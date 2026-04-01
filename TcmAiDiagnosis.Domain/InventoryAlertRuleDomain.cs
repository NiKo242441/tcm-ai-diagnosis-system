using Microsoft.EntityFrameworkCore;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.Constants;

namespace TcmAiDiagnosis.Domain
{
    /// <summary>
    /// 库存预警规则领域服务类
    /// </summary>
    public class InventoryAlertRuleDomain
    {
        private readonly TcmAiDiagnosisContext _context;

        public InventoryAlertRuleDomain(TcmAiDiagnosisContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 添加预警规则
        /// </summary>
        public async Task<InventoryAlertRule> AddAlertRuleAsync(InventoryAlertRule rule)
        {
            if (string.IsNullOrWhiteSpace(rule.RuleName))
                throw new ArgumentException("规则名称不能为空");

            if (rule.Threshold <= 0)
                throw new ArgumentException("阈值必须大于0");

            // 检查是否已存在同名规则
            var exists = await _context.InventoryAlertRules
                .AnyAsync((System.Linq.Expressions.Expression<Func<InventoryAlertRule, bool>>)(r => r.RuleName == rule.RuleName && r.TenantId == rule.TenantId));
            if (exists)
                throw new InvalidOperationException($"预警规则名称「{rule.RuleName}」已存在");

            rule.CreatedAt = DateTime.Now;
            rule.UpdatedAt = DateTime.Now;
            rule.IsEnabled = true;

            _context.InventoryAlertRules.Add(rule);
            await _context.SaveChangesAsync();

            return rule;
        }

        /// <summary>
        /// 更新预警规则
        /// </summary>
        public async Task UpdateAlertRuleAsync(InventoryAlertRule updatedRule)
        {
            var existingRule = await _context.InventoryAlertRules
                .FirstOrDefaultAsync((System.Linq.Expressions.Expression<Func<InventoryAlertRule, bool>>)(r => r.Id == updatedRule.Id))
                ?? throw new KeyNotFoundException("未找到指定预警规则");

            // 检查名称是否重复
            if (existingRule.RuleName != updatedRule.RuleName && await _context.InventoryAlertRules
                .AnyAsync((System.Linq.Expressions.Expression<Func<InventoryAlertRule, bool>>)(r => r.RuleName == updatedRule.RuleName &&
                              r.Id != updatedRule.Id &&
                              r.TenantId == updatedRule.TenantId)))
            {
                throw new InvalidOperationException($"预警规则名称「{updatedRule.RuleName}」已存在");
            }

            // 更新字段
            existingRule.RuleName = updatedRule.RuleName;
            existingRule.HerbId = updatedRule.HerbId;
            existingRule.AlertType = updatedRule.AlertType;
            existingRule.Threshold = updatedRule.Threshold;
            existingRule.ComparisonOperator = updatedRule.ComparisonOperator;
            existingRule.NotifyUserIds = updatedRule.NotifyUserIds;
            existingRule.IsEnabled = updatedRule.IsEnabled;
            existingRule.Priority = updatedRule.Priority;
            existingRule.Remark = updatedRule.Remark;
            existingRule.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 根据ID获取预警规则
        /// </summary>
        public async Task<InventoryAlertRule?> GetAlertRuleByIdAsync(int id, bool includeHerb = false)
        {
            var query = _context.InventoryAlertRules.AsQueryable();

            if (includeHerb)
            {
                query = query.Include(r => r.Herb);
            }

            return await query.FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// 分页查询预警规则
        /// </summary>
        public async Task<PagedResult<InventoryAlertRule>> GetPagedAlertRulesAsync(PagedRequest request, int? tenantId = null, string? alertType = null, bool? isEnabled = null)
        {
            var query = _context.InventoryAlertRules
                .Include(r => r.Herb)
                .Include(r => r.Creator)
                .AsQueryable();

            if (tenantId.HasValue)
                query = query.Where(r => r.TenantId == tenantId);

            if (!string.IsNullOrEmpty(alertType))
                query = query.Where(r => r.AlertType == alertType);

            if (isEnabled.HasValue)
                query = query.Where(r => r.IsEnabled == isEnabled.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
            {
                var keyword = request.SearchKeyword.Trim();
                query = query.Where(r =>
                    r.RuleName.Contains(keyword) ||
                    (r.Herb != null && r.Herb.Name.Contains(keyword)) ||
                    (r.Remark != null && r.Remark.Contains(keyword)));
            }

            query = query.OrderByDescending(r => r.Priority)
                        .ThenBy(r => r.RuleName);

            return await query.ToPagedResultAsync(request.Page, request.PageSize);
        }

        /// <summary>
        /// 启用/禁用预警规则
        /// </summary>
        public async Task ToggleAlertRuleAsync(int id, bool isEnabled)
        {
            var rule = await _context.InventoryAlertRules
                .FirstOrDefaultAsync((System.Linq.Expressions.Expression<Func<InventoryAlertRule, bool>>)(r => r.Id == id))
                ?? throw new KeyNotFoundException("未找到指定预警规则");

            rule.IsEnabled = isEnabled;
            rule.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 删除预警规则
        /// </summary>
        public async Task DeleteAlertRuleAsync(int id)
        {
            var rule = await _context.InventoryAlertRules
                .FirstOrDefaultAsync((System.Linq.Expressions.Expression<Func<InventoryAlertRule, bool>>)(r => r.Id == id))
                ?? throw new KeyNotFoundException("未找到指定预警规则");

            _context.InventoryAlertRules.Remove((InventoryAlertRule)rule);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 检查库存预警
        /// </summary>
        public async Task<List<InventoryAlertResult>> CheckInventoryAlertsAsync(int? tenantId = null)
        {
            var alerts = new List<InventoryAlertResult>();

            var enabledRules = await _context.InventoryAlertRules
                .Include(r => r.Herb)
                .Where(r => r.TenantId == tenantId && r.IsEnabled)
                .ToListAsync();

            var inventoryItems = await _context.InventoryItems
                .Include(i => i.Herb)
                .Where(i => i.TenantId == tenantId && i.IsActive)
                .ToListAsync();

            foreach (var rule in enabledRules)
            {
                var matchedItems = inventoryItems.Where(item =>
                    rule.HerbId == null || item.HerbId == rule.HerbId);

                foreach (var item in matchedItems)
                {
                    if (CheckRuleCondition(item, rule))
                    {
                        alerts.Add(new InventoryAlertResult
                        {
                            RuleId = rule.Id,
                            RuleName = rule.RuleName,
                            InventoryItemId = item.Id,
                            HerbName = item.Herb?.Name,
                            BatchNumber = item.BatchNumber,
                            AlertType = rule.AlertType,
                            CurrentValue = GetCurrentValue(item, rule),
                            Threshold = rule.Threshold,
                            AlertMessage = GenerateAlertMessage(item, rule),
                            TriggeredAt = DateTime.Now,
                            Priority = rule.Priority
                        });
                    }
                }
            }

            return alerts.OrderByDescending(a => a.Priority).ThenBy(a => a.HerbName).ToList();
        }

        /// <summary>
        /// 获取指定药材的相关预警规则
        /// </summary>
        public async Task<List<InventoryAlertRule>> GetAlertRulesByHerbAsync(int herbId, int? tenantId = null)
        {
            return await _context.InventoryAlertRules
                .Include(r => r.Herb)
                .Where(r => r.TenantId == tenantId &&
                           (r.HerbId == null || r.HerbId == herbId) &&
                           r.IsEnabled)
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.RuleName)
                .ToListAsync();
        }

        /// <summary>
        /// 批量更新规则状态
        /// </summary>
        public async Task BatchUpdateRuleStatusAsync(List<int> ruleIds, bool isEnabled)
        {
            var rules = await _context.InventoryAlertRules
                .Where(r => ruleIds.Contains(r.Id))
                .ToListAsync();

            foreach (var rule in rules)
            {
                rule.IsEnabled = isEnabled;
                rule.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        private bool CheckRuleCondition(InventoryItem item, InventoryAlertRule rule)
        {
            var currentValue = GetCurrentValue(item, rule);

            return rule.ComparisonOperator switch
            {
                InventoryConstants.ComparisonOperator.LessThan => currentValue < rule.Threshold,
                InventoryConstants.ComparisonOperator.LessThanOrEqual => currentValue <= rule.Threshold,
                InventoryConstants.ComparisonOperator.GreaterThan => currentValue > rule.Threshold,
                InventoryConstants.ComparisonOperator.GreaterThanOrEqual => currentValue >= rule.Threshold,
                _ => false
            };
        }

        private decimal GetCurrentValue(InventoryItem item, InventoryAlertRule rule)
        {
            return rule.AlertType switch
            {
                InventoryConstants.AlertType.LowStock => item.CurrentQuantity,
                InventoryConstants.AlertType.Expiring => item.ExpiryDate.HasValue ?
                    (decimal)(item.ExpiryDate.Value - DateTime.Now).Days : decimal.MaxValue,
                InventoryConstants.AlertType.Overstock => item.CurrentQuantity,
                _ => 0
            };
        }

        private string GenerateAlertMessage(InventoryItem item, InventoryAlertRule rule)
        {
            return rule.AlertType switch
            {
                InventoryConstants.AlertType.LowStock => $"药材[{item.Herb?.Name}]库存不足，当前库存：{item.CurrentQuantity}{item.Unit}，阈值：{rule.Threshold}{item.Unit}",
                InventoryConstants.AlertType.Expiring => $"药材[{item.Herb?.Name}]即将过期，剩余天数：{(item.ExpiryDate.Value - DateTime.Now).Days}天，阈值：{rule.Threshold}天",
                InventoryConstants.AlertType.Overstock => $"药材[{item.Herb?.Name}]库存积压，当前库存：{item.CurrentQuantity}{item.Unit}，阈值：{rule.Threshold}{item.Unit}",
                _ => "未知预警类型"
            };
        }
    }

    /// <summary>
    /// 库存预警结果
    /// </summary>
    public class InventoryAlertResult
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public int InventoryItemId { get; set; }
        public string? HerbName { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal Threshold { get; set; }
        public string AlertMessage { get; set; } = string.Empty;
        public DateTime TriggeredAt { get; set; }
        public int Priority { get; set; }
    }
}