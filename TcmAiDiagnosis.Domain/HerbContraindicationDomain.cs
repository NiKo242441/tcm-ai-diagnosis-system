using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TcmAiDiagnosis.EFContext; // 使用正确的命名空间
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Domain
{
    public class HerbContraindicationDomain
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly ILogger<HerbContraindicationDomain> _logger;

        public HerbContraindicationDomain(
            TcmAiDiagnosisContext context,
            ILogger<HerbContraindicationDomain> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 检查药材是否存在
        /// </summary>
        public async Task<bool> CheckHerbExistsAsync(int herbId)
        {
            return await _context.Herbs
                .AnyAsync(h => h.Id == herbId && h.IsDeleted);
        }

        /// <summary>
        /// 添加配伍禁忌记录
        /// </summary>
        public async Task<HerbContraindication> AddContraindicationAsync(HerbContraindication contraindication)
        {
            // 检查是否已存在相同的配伍禁忌
            var exists = await _context.HerbContraindications
                .AnyAsync(hc => hc.PrimaryHerbId == contraindication.PrimaryHerbId &&
                               hc.ConflictHerbId == contraindication.ConflictHerbId &&
                               hc.IsActive);

            if (exists)
            {
                throw new InvalidOperationException("该配伍禁忌记录已存在");
            }

            // 检查反向记录是否存在（A-B 和 B-A 视为相同）
            var reverseExists = await _context.HerbContraindications
                .AnyAsync(hc => hc.PrimaryHerbId == contraindication.ConflictHerbId &&
                               hc.ConflictHerbId == contraindication.PrimaryHerbId &&
                               hc.IsActive);

            if (reverseExists)
            {
                throw new InvalidOperationException("反向配伍禁忌记录已存在");
            }

            _context.HerbContraindications.Add(contraindication);
            await _context.SaveChangesAsync();

            // 加载关联的药材信息
            await _context.Entry(contraindication)
                .Reference(hc => hc.PrimaryHerb)
                .LoadAsync();

            await _context.Entry(contraindication)
                .Reference(hc => hc.ConflictHerb)
                .LoadAsync();

            return contraindication;
        }

        /// <summary>
        /// 更新配伍禁忌记录
        /// </summary>
        public async Task UpdateContraindicationAsync(HerbContraindication contraindication)
        {
            var existing = await _context.HerbContraindications
                .FirstOrDefaultAsync(hc => hc.ContraindicationId == contraindication.ContraindicationId);

            if (existing == null)
            {
                throw new KeyNotFoundException("未找到指定的配伍禁忌记录");
            }

            // 检查是否与其他记录冲突（排除自身）
            var conflictExists = await _context.HerbContraindications
                .AnyAsync(hc => hc.ContraindicationId != contraindication.ContraindicationId &&
                               ((hc.PrimaryHerbId == contraindication.PrimaryHerbId &&
                                 hc.ConflictHerbId == contraindication.ConflictHerbId) ||
                                (hc.PrimaryHerbId == contraindication.ConflictHerbId &&
                                 hc.ConflictHerbId == contraindication.PrimaryHerbId)) &&
                               hc.IsActive);

            if (conflictExists)
            {
                throw new InvalidOperationException("更新后的记录与其他配伍禁忌记录冲突");
            }

            // 更新字段
            existing.PrimaryHerbId = contraindication.PrimaryHerbId;
            existing.ConflictHerbId = contraindication.ConflictHerbId;
            existing.ContraindicationType = contraindication.ContraindicationType;
            existing.RiskLevel = contraindication.RiskLevel;
            existing.Description = contraindication.Description;
            existing.AdverseReactions = contraindication.AdverseReactions;
            existing.Evidence = contraindication.Evidence;
            existing.IsAbsoluteContraindication = contraindication.IsAbsoluteContraindication;
            existing.SpecialNotes = contraindication.SpecialNotes;
            existing.AlternativeSuggestions = contraindication.AlternativeSuggestions;
            existing.IsActive = contraindication.IsActive;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 根据ID获取配伍禁忌记录
        /// </summary>
        public async Task<HerbContraindication?> GetContraindicationByIdAsync(int contraindicationId)
        {
            return await _context.HerbContraindications
                .Include(hc => hc.PrimaryHerb)
                .Include(hc => hc.ConflictHerb)
                .FirstOrDefaultAsync(hc => hc.ContraindicationId == contraindicationId);
        }

        /// <summary>
        /// 分页查询配伍禁忌记录
        /// </summary>
        public async Task<PagedResult<HerbContraindication>> QueryContraindicationsAsync(HerbContraindicationQueryDto query)
        {
            var queryable = _context.HerbContraindications
                .Include(hc => hc.PrimaryHerb)
                .Include(hc => hc.ConflictHerb)
                .AsQueryable();

            // 应用筛选条件
            if (query.PrimaryHerbId.HasValue)
            {
                queryable = queryable.Where(hc => hc.PrimaryHerbId == query.PrimaryHerbId.Value);
            }

            if (query.ConflictHerbId.HasValue)
            {
                queryable = queryable.Where(hc => hc.ConflictHerbId == query.ConflictHerbId.Value);
            }

            if (!string.IsNullOrEmpty(query.HerbName))
            {
                queryable = queryable.Where(hc =>
                    hc.PrimaryHerb.Name.Contains(query.HerbName) ||
                    hc.ConflictHerb.Name.Contains(query.HerbName));
            }

            if (!string.IsNullOrEmpty(query.ContraindicationType))
            {
                queryable = queryable.Where(hc => hc.ContraindicationType == query.ContraindicationType);
            }

            if (query.RiskLevel.HasValue)
            {
                queryable = queryable.Where(hc => hc.RiskLevel == query.RiskLevel.Value);
            }

            if (query.IsAbsoluteContraindication.HasValue)
            {
                queryable = queryable.Where(hc => hc.IsAbsoluteContraindication == query.IsAbsoluteContraindication.Value);
            }

            if (query.IsActive.HasValue)
            {
                queryable = queryable.Where(hc => hc.IsActive == query.IsActive.Value);
            }

            if (query.ReviewStatus.HasValue)
            {
                queryable = queryable.Where(hc => hc.ReviewStatus == query.ReviewStatus.Value);
            }

            // 获取总数
            var totalCount = await queryable.CountAsync();

            // 分页
            var items = await queryable
                .OrderByDescending(hc => hc.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<HerbContraindication>
            {
                TotalCount = totalCount,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                Items = items
            };
        }

        /// <summary>
        /// 检查药材组合的配伍禁忌
        /// </summary>
        public async Task<ContraindicationCheckResult> CheckContraindicationsAsync(List<int> herbIds)
        {
            if (herbIds == null || herbIds.Count < 2)
            {
                throw new ArgumentException("至少需要2个药材ID进行检查");
            }

            // 检查所有药材是否存在
            var existingHerbIds = await _context.Herbs
                .Where(h => herbIds.Contains(h.Id) && h.IsDeleted)
                .Select(h => h.Id)
                .ToListAsync();

            var missingHerbIds = herbIds.Except(existingHerbIds).ToList();
            if (missingHerbIds.Any())
            {
                throw new ArgumentException($"以下药材ID不存在或已禁用: {string.Join(", ", missingHerbIds)}");
            }

            // 查询所有相关的配伍禁忌
            var contraindications = await _context.HerbContraindications
                .Include(hc => hc.PrimaryHerb)
                .Include(hc => hc.ConflictHerb)
                .Where(hc => hc.IsActive && hc.ReviewStatus == 1) // 只检查已审核的启用记录
                .Where(hc => herbIds.Contains(hc.PrimaryHerbId) && herbIds.Contains(hc.ConflictHerbId))
                .ToListAsync();

            var result = new ContraindicationCheckResult
            {
                HasContraindications = contraindications.Any(),
                Contraindications = new List<HerbContraindicationDto>(),
                MaxRiskLevel = contraindications.Any() ? contraindications.Max(hc => hc.RiskLevel) : 0,
                Recommendations = new List<string>()
            };

            // 转换为DTO
            foreach (var contraindication in contraindications)
            {
                result.Contraindications.Add(new HerbContraindicationDto
                {
                    ContraindicationId = contraindication.ContraindicationId,
                    PrimaryHerbId = contraindication.PrimaryHerbId,
                    PrimaryHerbName = contraindication.PrimaryHerb?.Name ?? "",
                    ConflictHerbId = contraindication.ConflictHerbId,
                    ConflictHerbName = contraindication.ConflictHerb?.Name ?? "",
                    ContraindicationType = contraindication.ContraindicationType,
                    RiskLevel = contraindication.RiskLevel,
                    RiskLevelDescription = GetRiskLevelDescription(contraindication.RiskLevel),
                    Description = contraindication.Description,
                    AdverseReactions = contraindication.AdverseReactions,
                    Evidence = contraindication.Evidence,
                    IsAbsoluteContraindication = contraindication.IsAbsoluteContraindication,
                    SpecialNotes = contraindication.SpecialNotes,
                    AlternativeSuggestions = contraindication.AlternativeSuggestions,
                    IsActive = contraindication.IsActive,
                    ReviewStatus = contraindication.ReviewStatus,
                    ReviewStatusDescription = GetReviewStatusDescription(contraindication.ReviewStatus),
                    ReviewComments = contraindication.ReviewComments,
                    CreatedAt = contraindication.CreatedAt,
                    UpdatedAt = contraindication.UpdatedAt
                });
            }

            // 生成风险评估摘要和建议
            result.RiskSummary = GenerateRiskSummary(result);
            result.Recommendations = GenerateRecommendations(result);

            return result;
        }

        /// <summary>
        /// 审核配伍禁忌记录
        /// </summary>
        public async Task ReviewContraindicationAsync(int contraindicationId, int reviewStatus, string? reviewComments)
        {
            var contraindication = await _context.HerbContraindications
                .FirstOrDefaultAsync(hc => hc.ContraindicationId == contraindicationId);

            if (contraindication == null)
            {
                throw new KeyNotFoundException("未找到指定的配伍禁忌记录");
            }

            contraindication.ReviewStatus = reviewStatus;
            contraindication.ReviewComments = reviewComments;
            contraindication.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 获取药材的所有配伍禁忌
        /// </summary>
        public async Task<List<HerbContraindication>> GetHerbContraindicationsAsync(int herbId)
        {
            return await _context.HerbContraindications
                .Include(hc => hc.PrimaryHerb)
                .Include(hc => hc.ConflictHerb)
                .Where(hc => hc.IsActive && hc.ReviewStatus == 1 &&
                            (hc.PrimaryHerbId == herbId || hc.ConflictHerbId == herbId))
                .ToListAsync();
        }

        /// <summary>
        /// 批量导入配伍禁忌
        /// </summary>
        public async Task<(int successCount, int skipCount)> BatchImportContraindicationsAsync(
            List<HerbContraindication> contraindications)
        {
            int successCount = 0;
            int skipCount = 0;

            foreach (var contraindication in contraindications)
            {
                try
                {
                    // 检查药材是否存在
                    var primaryHerbExists = await CheckHerbExistsAsync(contraindication.PrimaryHerbId);
                    var conflictHerbExists = await CheckHerbExistsAsync(contraindication.ConflictHerbId);

                    if (!primaryHerbExists || !conflictHerbExists)
                    {
                        skipCount++;
                        continue;
                    }

                    // 检查是否已存在
                    var exists = await _context.HerbContraindications
                        .AnyAsync(hc => hc.PrimaryHerbId == contraindication.PrimaryHerbId &&
                                       hc.ConflictHerbId == contraindication.ConflictHerbId &&
                                       hc.IsActive);

                    if (exists)
                    {
                        skipCount++;
                        continue;
                    }

                    contraindication.CreatedAt = DateTime.Now;
                    contraindication.UpdatedAt = DateTime.Now;

                    _context.HerbContraindications.Add(contraindication);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "导入配伍禁忌失败: {PrimaryHerbId}-{ConflictHerbId}",
                        contraindication.PrimaryHerbId, contraindication.ConflictHerbId);
                    skipCount++;
                }
            }

            await _context.SaveChangesAsync();
            return (successCount, skipCount);
        }

        // 私有辅助方法
        private string GetRiskLevelDescription(int riskLevel)
        {
            return riskLevel switch
            {
                1 => "低风险",
                2 => "中风险",
                3 => "高风险",
                4 => "严重风险",
                _ => "未知风险"
            };
        }

        private string GetReviewStatusDescription(int reviewStatus)
        {
            return reviewStatus switch
            {
                0 => "待审核",
                1 => "已审核",
                2 => "审核不通过",
                _ => "未知状态"
            };
        }

        private string GenerateRiskSummary(ContraindicationCheckResult result)
        {
            if (!result.HasContraindications)
            {
                return "未发现配伍禁忌，可安全使用";
            }

            var maxRisk = result.MaxRiskLevel;
            return maxRisk switch
            {
                1 => "存在低风险配伍禁忌，建议关注",
                2 => "存在中风险配伍禁忌，需要谨慎使用",
                3 => "存在高风险配伍禁忌，不建议使用",
                4 => "存在严重配伍禁忌，禁止使用",
                _ => "存在配伍禁忌风险"
            };
        }

        private List<string> GenerateRecommendations(ContraindicationCheckResult result)
        {
            var recommendations = new List<string>();

            if (!result.HasContraindications)
            {
                recommendations.Add("当前药材组合无配伍禁忌，可正常使用");
                return recommendations;
            }

            var absoluteContraindications = result.Contraindications
                .Where(c => c.IsAbsoluteContraindication)
                .ToList();

            if (absoluteContraindications.Any())
            {
                recommendations.Add("存在绝对配伍禁忌，强烈建议更换药材");
            }

            var highRiskContraindications = result.Contraindications
                .Where(c => c.RiskLevel >= 3)
                .ToList();

            if (highRiskContraindications.Any())
            {
                recommendations.Add("存在高风险配伍禁忌，不建议同时使用");
            }

            // 添加具体的替代建议
            foreach (var contraindication in result.Contraindications.Where(c => !string.IsNullOrEmpty(c.AlternativeSuggestions)))
            {
                recommendations.Add($"替代建议: {contraindication.AlternativeSuggestions}");
            }

            if (recommendations.Count == 0)
            {
                recommendations.Add("存在配伍禁忌，建议咨询专业中医师");
            }

            return recommendations;
        }
    }
}