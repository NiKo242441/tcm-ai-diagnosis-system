using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Domain.Services
{
    /// <summary>
    /// 系统自动初审服务（同步）
    /// - 实现十八反、十九畏等配伍禁忌检查
    /// - 结合药性/毒性等基本规则进行风险分级
    /// - 整合AI生成的中药安全警告作为高权重参考
    /// </summary>
    public class SystemAutoCheckService
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly ILogger<SystemAutoCheckService> _logger;

        public SystemAutoCheckService(TcmAiDiagnosisContext context, ILogger<SystemAutoCheckService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 执行同步自动初审
        /// </summary>
        public async Task<SystemCheckResultDto> PerformSystemCheckAsync(TreatmentDto editDto)
        {
            var result = new SystemCheckResultDto
            {
                RiskLevel = RiskLevel.Low,
                RequiresVerification = false,
                VerificationType = string.Empty,
                Message = "初审完成"
            };

            try
            {
                _logger.LogInformation("开始执行系统自动初审，TreatmentId={TreatmentId}", editDto.Id);

                // 1) 基础：整合AI生成的中药安全警告
                var aiSevereWarningExists = editDto.HerbalWarnings.Any(w =>
                    string.Equals(w.SeverityLevel, "严重", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(w.SeverityLevel, "高", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(w.SeverityLevel, "critical", StringComparison.OrdinalIgnoreCase));
                if (aiSevereWarningExists)
                {
                    result.Warnings.Add("AI警告标记为严重或高风险");
                    result.RiskLevel = MaxRisk(result.RiskLevel, RiskLevel.High);
                }

                // 2) 十八反/十九畏配伍禁忌（绝对禁忌 → 高风险）
                var absolutePairs = await GetAbsoluteContraPairsAsync();
                var allHerbNames = editDto.Prescriptions
                    .SelectMany(p => p.Items.Select(i => NormalizeHerbName(i.Name)))
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .ToList();
                foreach (var pair in absolutePairs)
                {
                    if (allHerbNames.Contains(pair.Item1) && allHerbNames.Contains(pair.Item2))
                    {
                        result.Warnings.Add($"发现绝对禁忌配伍：{pair.Item1} 与 {pair.Item2}");
                        result.RiskLevel = MaxRisk(result.RiskLevel, RiskLevel.High);
                    }
                }

                // 3) 药性与毒性（相对禁忌与毒性超量 → 中/高风险）
                // 根据处方药材名称，从药材库查询毒性与常用剂量范围
                var herbDict = await _context.Herbs
                    .Where(h => allHerbNames.Contains(NormalizeHerbName(h.Name)))
                    .ToDictionaryAsync(h => NormalizeHerbName(h.Name), h => h);

                foreach (var p in editDto.Prescriptions)
                {
                    foreach (var item in p.Items)
                    {
                        var name = NormalizeHerbName(item.Name);
                        if (herbDict.TryGetValue(name, out var herbDef))
                        {
                            // 毒性等级
                            if (string.Equals(herbDef.ToxicityLevel, "有毒", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(herbDef.ToxicityLevel, "大毒", StringComparison.OrdinalIgnoreCase))
                            {
                                result.Warnings.Add($"药材[{herbDef.Name}]毒性等级：{herbDef.ToxicityLevel}");
                                result.RiskLevel = MaxRisk(result.RiskLevel, RiskLevel.Medium);
                            }

                            // 简易剂量校验：若存在纯数字剂量，尝试与常用范围进行粗略比较
                            if (TryParseDosage(item.Dosage, out var dosageValue) && TryParseRange(herbDef.DosageRange, out var min, out var max))
                            {
                                if (dosageValue > max)
                                {
                                    result.Warnings.Add($"药材[{herbDef.Name}]剂量可能超过常用上限({max}{herbDef.CommonUnit})");
                                    result.RiskLevel = MaxRisk(result.RiskLevel, RiskLevel.High);
                                }
                            }
                        }
                    }
                }

                // 4) 风险验证要求（四级验证映射）
                switch (result.RiskLevel)
                {
                    case RiskLevel.Low:
                        result.RequiresVerification = false;
                        result.VerificationType = string.Empty;
                        result.Message = "低风险：自动通过，提交药剂师复核。";
                        break;
                    case RiskLevel.Medium:
                        result.RequiresVerification = true;
                        result.VerificationType = "MediumConfirm";
                        result.Message = "中风险：需要医生确认后提交。";
                        break;
                    case RiskLevel.High:
                        result.RequiresVerification = true;
                        result.VerificationType = "HighPassword";
                        result.Message = "高风险：需要医生密码验证后提交。";
                        break;
                    case RiskLevel.Critical:
                        result.RequiresVerification = true;
                        result.VerificationType = "CriticalPassword";
                        result.Message = "极高风险：需要密码与理由后提交。";
                        break;
                }

                _logger.LogInformation("系统自动初审完成，TreatmentId={TreatmentId}，RiskLevel={RiskLevel}", editDto.Id, result.RiskLevel);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "系统自动初审失败，TreatmentId={TreatmentId}", editDto.Id);
                // 失败时默认返回中风险并要求确认
                return new SystemCheckResultDto
                {
                    RiskLevel = RiskLevel.Medium,
                    RequiresVerification = true,
                    VerificationType = "MediumConfirm",
                    Warnings = new List<string> { "系统初审失败：请人工确认后再提交。" },
                    Message = "初审发生异常"
                };
            }
        }

        // --- 私有工具方法 ---

        private static string NormalizeHerbName(string? name)
        {
            return (name ?? string.Empty).Trim().ToLowerInvariant();
        }

        /// <summary>
        /// 从数据库获取绝对禁忌的配伍对
        /// 查询条件：IsAbsoluteContraindication = true 且 IsActive = true
        /// </summary>
        private async Task<List<(string, string)>> GetAbsoluteContraPairsAsync()
        {
            try
            {
                // 使用 Join 在数据库层面过滤已删除的药材，提高查询性能
                var contraindications = await (
                    from hc in _context.HerbContraindications
                    join primaryHerb in _context.Herbs on hc.PrimaryHerbId equals primaryHerb.Id
                    join conflictHerb in _context.Herbs on hc.ConflictHerbId equals conflictHerb.Id
                    where hc.IsAbsoluteContraindication 
                        && hc.IsActive 
                        && !primaryHerb.IsDeleted
                        && !conflictHerb.IsDeleted
                    select new
                    {
                        PrimaryHerbName = primaryHerb.Name,
                        ConflictHerbName = conflictHerb.Name
                    }
                ).ToListAsync();

                var pairs = contraindications
                    .Select(hc => (
                        NormalizeHerbName(hc.PrimaryHerbName),
                        NormalizeHerbName(hc.ConflictHerbName)
                    ))
                    .Distinct()
                    .ToList();

                _logger.LogInformation("从数据库加载绝对禁忌配伍对，共 {Count} 组", pairs.Count);
                return pairs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从数据库加载绝对禁忌配伍对失败，使用空列表");
                // 发生异常时返回空列表，避免影响主流程
                return new List<(string, string)>();
            }
        }

        private static RiskLevel MaxRisk(RiskLevel a, RiskLevel b)
        {
            return (RiskLevel)Math.Max((int)a, (int)b);
        }

        private static bool TryParseDosage(string? dosage, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(dosage)) return false;
            // 仅提取前导数字，如"10g" -> 10
            var digits = new string(dosage.TakeWhile(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
            digits = digits.Replace(',', '.');
            return double.TryParse(digits, out value);
        }

        private static bool TryParseRange(string? range, out double min, out double max)
        {
            min = 0; max = 0;
            if (string.IsNullOrWhiteSpace(range)) return false;
            // 简单格式："3-10" 或 "3~10" 或 "3—10"
            var normalized = range.Replace("~", "-").Replace("—", "-").Trim();
            var parts = normalized.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && double.TryParse(parts[0], out var a) && double.TryParse(parts[1], out var b))
            {
                min = Math.Min(a, b);
                max = Math.Max(a, b);
                return true;
            }
            return false;
        }
    }
}