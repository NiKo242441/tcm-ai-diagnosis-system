using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Pages.Inventory
{
    public class InventoryAlertRulePageModel : PageModel
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly InventoryAlertRuleDomain _alertRuleDomain;
        private readonly ILogger<InventoryAlertRulePageModel> _logger;

        public InventoryAlertRulePageModel(
            TcmAiDiagnosisContext context,
            InventoryAlertRuleDomain alertRuleDomain,
            ILogger<InventoryAlertRulePageModel> logger)
        {
            _context = context;
            _alertRuleDomain = alertRuleDomain;
            _logger = logger;
        }

        public List<InventoryAlertRule> Rules { get; set; } = new();
        public List<TcmAiDiagnosis.Entities.Herb> Herbs { get; set; } = new();

        [BindProperty]
        public InventoryAlertRule EditingRule { get; set; } = new();

        [BindProperty]
        public string SearchKeyword { get; set; } = string.Empty;

        [BindProperty]
        public string SelectedAlertType { get; set; } = "all";

        [BindProperty]
        public string SelectedStatus { get; set; } = "all";

        // 新增：用于状态切换的绑定属性
        [BindProperty]
        public int ToggleId { get; set; }

        [BindProperty]
        public bool ToggleIsEnabled { get; set; }

        public bool ShowModal { get; set; }
        public string ModalTitle => EditingRule.Id > 0 ? "编辑预警规则" : "创建预警规则";

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await LoadHerbsAsync();
                await LoadRulesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载数据失败");
                Herbs = new List<TcmAiDiagnosis.Entities.Herb>();
                Rules = new List<InventoryAlertRule>();
                TempData["ErrorMessage"] = "加载数据失败，请刷新页面";
            }
        }

        private async Task LoadHerbsAsync()
        {
            Herbs = await _context.Herbs
                .Where(h => !h.IsDeleted)  // 修正：使用 IsDeleted 而不是 IsActive
                .OrderBy(h => h.Name)      // 修正：使用 Name 而不是 HerbName
                .AsNoTracking()
                .ToListAsync();
        }

        private async Task LoadRulesAsync()
        {
            var request = new PagedRequest
            {
                Page = 1,
                PageSize = 50,
                SearchKeyword = SearchKeyword
            };

            bool? isEnabled = SelectedStatus switch
            {
                "enabled" => true,
                "disabled" => false,
                _ => null
            };

            var result = await _alertRuleDomain.GetPagedAlertRulesAsync(
                request,
                tenantId: GetCurrentTenantId(),
                alertType: SelectedAlertType == "all" ? null : SelectedAlertType,
                isEnabled: isEnabled);

            Rules = result?.Items?.ToList() ?? new List<InventoryAlertRule>();
        }

        public async Task<IActionResult> OnGetCreate()
        {
            EditingRule = new InventoryAlertRule
            {
                IsEnabled = true,
                Priority = 2,
                ComparisonOperator = "LT",
                TenantId = GetCurrentTenantId(),
                CreatedBy = GetCurrentUserId()
            };
            ShowModal = true;
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetEdit(int id)
        {
            try
            {
                var rule = await _alertRuleDomain.GetAlertRuleByIdAsync(id, true);
                if (rule == null)
                {
                    TempData["ErrorMessage"] = "未找到指定的预警规则";
                    return RedirectToPage();
                }

                EditingRule = rule;
                ShowModal = true;
                await LoadDataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载规则失败");
                TempData["ErrorMessage"] = "加载规则失败";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostSave()
        {
            _logger.LogInformation("开始保存预警规则，ID: {Id}", EditingRule.Id);

            // 清除搜索字段的模型状态，避免验证错误
            ModelState.Remove("SearchKeyword");
            ModelState.Remove("SelectedAlertType");
            ModelState.Remove("SelectedStatus");
            ModelState.Remove("ToggleId");
            ModelState.Remove("ToggleIsEnabled");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("模型验证失败");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("验证错误: {Error}", error.ErrorMessage);
                }
                ShowModal = true;
                await LoadDataAsync();
                return Page();
            }

            try
            {
                if (EditingRule.Id == 0)
                {
                    _logger.LogInformation("创建新规则: {RuleName}", EditingRule.RuleName);

                    EditingRule.TenantId = GetCurrentTenantId();
                    EditingRule.CreatedBy = GetCurrentUserId();
                    EditingRule.CreatedAt = DateTime.Now;
                    EditingRule.UpdatedAt = DateTime.Now;

                    await _alertRuleDomain.AddAlertRuleAsync(EditingRule);
                    TempData["SuccessMessage"] = "预警规则创建成功";
                    _logger.LogInformation("预警规则创建成功，ID: {Id}", EditingRule.Id);
                }
                else
                {
                    _logger.LogInformation("更新规则: {RuleName} (ID: {Id})", EditingRule.RuleName, EditingRule.Id);
                    EditingRule.UpdatedAt = DateTime.Now;
                    await _alertRuleDomain.UpdateAlertRuleAsync(EditingRule);
                    TempData["SuccessMessage"] = "预警规则更新成功";
                    _logger.LogInformation("预警规则更新成功");
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存预警规则失败");
                ModelState.AddModelError("", $"保存失败: {ex.Message}");
                ShowModal = true;
                await LoadDataAsync();
                return Page();
            }
        }

        // 修复后的状态切换方法
        public async Task<IActionResult> OnPostToggle()
        {
            try
            {
                _logger.LogInformation("=== 开始切换规则状态 ===");
                _logger.LogInformation("接收参数 - ToggleId: {ToggleId}, ToggleIsEnabled: {ToggleIsEnabled}",
                    ToggleId, ToggleIsEnabled);

                if (ToggleId <= 0)
                {
                    _logger.LogWarning("无效的规则ID: {ToggleId}", ToggleId);
                    TempData["ErrorMessage"] = "无效的规则ID";
                    return RedirectToPage();
                }

                _logger.LogInformation("调用领域服务切换状态...");
                await _alertRuleDomain.ToggleAlertRuleAsync(ToggleId, ToggleIsEnabled);

                _logger.LogInformation("状态切换成功");
                TempData["SuccessMessage"] = ToggleIsEnabled ? "规则已启用" : "规则已禁用";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切换规则状态失败");
                TempData["ErrorMessage"] = $"操作失败: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            try
            {
                _logger.LogInformation("删除规则: ID={Id}", id);
                await _alertRuleDomain.DeleteAlertRuleAsync(id);
                TempData["SuccessMessage"] = "规则删除成功";
                _logger.LogInformation("规则删除成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除规则失败");
                TempData["ErrorMessage"] = "删除失败";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckAlerts()
        {
            try
            {
                _logger.LogInformation("开始检查库存预警");
                var alerts = await _alertRuleDomain.CheckInventoryAlertsAsync(GetCurrentTenantId());
                TempData["InfoMessage"] = $"发现 {alerts.Count} 个预警";
                _logger.LogInformation("库存预警检查完成，发现 {Count} 个预警", alerts.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查预警失败");
                TempData["ErrorMessage"] = "检查预警失败";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostTestCreate()
        {
            _logger.LogInformation("=== 开始测试创建 ===");

            try
            {
                var testRule = new InventoryAlertRule
                {
                    RuleName = "测试规则_" + DateTime.Now.Ticks,
                    AlertType = "LowStock",
                    Threshold = 10,
                    ComparisonOperator = "LT",
                    Priority = 2,
                    IsEnabled = true,
                    TenantId = GetCurrentTenantId(),
                    CreatedBy = GetCurrentUserId(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var result = await _alertRuleDomain.AddAlertRuleAsync(testRule);
                TempData["SuccessMessage"] = "测试规则创建成功";
                _logger.LogInformation("测试规则创建成功，ID: {Id}", result.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试创建失败");
                TempData["ErrorMessage"] = $"测试创建失败: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetCloseModal()
        {
            return RedirectToPage();
        }

        // 辅助方法
        public string GetAlertTypeBadge(string alertType) => alertType switch
        {
            "LowStock" => "bg-danger",
            "Expiring" => "bg-warning",
            "Overstock" => "bg-info",
            _ => "bg-secondary"
        };

        public string GetAlertTypeText(string alertType) => alertType switch
        {
            "LowStock" => "低库存",
            "Expiring" => "临期预警",
            "Overstock" => "库存积压",
            _ => alertType
        };

        public string GetPriorityBadge(int priority) => priority switch
        {
            3 => "bg-danger",
            2 => "bg-warning",
            1 => "bg-info",
            _ => "bg-secondary"
        };

        public string GetPriorityText(int priority) => priority switch
        {
            3 => "高",
            2 => "中",
            1 => "低",
            _ => priority.ToString()
        };

        public string GetStatusBadge(bool isEnabled) =>
            isEnabled ? "bg-success" : "bg-secondary";

        public string GetStatusText(bool isEnabled) =>
            isEnabled ? "启用" : "禁用";

        public string GetOperatorText(string comparisonOperator) => comparisonOperator switch
        {
            "LT" => "小于",
            "LTE" => "小于等于",
            "GT" => "大于",
            "GTE" => "大于等于",
            "EQ" => "等于",
            _ => comparisonOperator
        };

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 1;
        }

        private int GetCurrentTenantId()
        {
            var tenantIdClaim = User.FindFirst("TenantId");
            return tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out int tenantId) ? tenantId : 1;
        }
    }
}