using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Pages.Inventory
{
    public class InventoryAlertsPageModel : PageModel
    {
        private readonly InventoryAlertRuleDomain _alertRuleDomain;
        private readonly ILogger<InventoryAlertsPageModel> _logger;

        public InventoryAlertsPageModel(
            InventoryAlertRuleDomain alertRuleDomain,
            ILogger<InventoryAlertsPageModel> logger)
        {
            _alertRuleDomain = alertRuleDomain;
            _logger = logger;
        }

        public List<InventoryAlertResult> CurrentAlerts { get; set; } = new();

        // 使用 Priority 字段来统计
        public int CriticalAlertsCount => CurrentAlerts.Count(a => a.Priority == 3);
        public int HighAlertsCount => CurrentAlerts.Count(a => a.Priority == 2);
        public int MediumAlertsCount => CurrentAlerts.Count(a => a.Priority == 1);
        public int TotalAlertsCount => CurrentAlerts.Count;

        public async Task OnGetAsync()
        {
            _logger.LogInformation("加载库存预警监控页面");
            await LoadAlertsAsync();
        }

        public async Task<IActionResult> OnPostRefreshAsync()
        {
            _logger.LogInformation("手动刷新预警列表");
            await LoadAlertsAsync();
            TempData["SuccessMessage"] = "预警列表已刷新";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckAlertsAsync()
        {
            try
            {
                _logger.LogInformation("开始检查库存预警");
                await LoadAlertsAsync();

                var message = TotalAlertsCount > 0
                    ? $"发现 {TotalAlertsCount} 个预警，请及时处理"
                    : "当前无预警信息";

                TempData["SuccessMessage"] = message;
                _logger.LogInformation("预警检查完成: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查预警失败");
                TempData["ErrorMessage"] = "预警检查失败: " + ex.Message;
            }
            return RedirectToPage();
        }

        // 强制刷新方法
        public async Task<IActionResult> OnGetForceRefreshAsync()
        {
            try
            {
                _logger.LogInformation("开始强制刷新预警数据");

                // 重置当前预警列表
                CurrentAlerts = new List<InventoryAlertResult>();

                // 强制重新加载
                await LoadAlertsAsync(true);

                TempData["SuccessMessage"] = "预警数据已强制刷新";
                _logger.LogInformation("强制刷新完成，共 {Count} 个预警", CurrentAlerts.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "强制刷新失败");
                TempData["ErrorMessage"] = "强制刷新失败: " + ex.Message;
            }
            return Page();
        }

        // 重置状态方法
        public async Task<IActionResult> OnGetResetAsync()
        {
            try
            {
                _logger.LogInformation("重置预警页面状态");

                // 完全重置所有状态
                CurrentAlerts = new List<InventoryAlertResult>();

                // 重新加载数据
                await LoadAlertsAsync();

                TempData["SuccessMessage"] = "页面状态已重置";
                _logger.LogInformation("页面状态重置完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重置状态失败");
                TempData["ErrorMessage"] = "重置状态失败: " + ex.Message;
            }
            return Page();
        }

        // 清除所有预警
        public async Task<IActionResult> OnPostClearAllAsync()
        {
            try
            {
                _logger.LogInformation("清除所有预警显示");
                CurrentAlerts = new List<InventoryAlertResult>();
                TempData["SuccessMessage"] = "已清除所有预警显示";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除预警失败");
                TempData["ErrorMessage"] = "清除预警失败: " + ex.Message;
            }
            return RedirectToPage();
        }

        // 加载预警数据
        private async Task LoadAlertsAsync(bool forceRefresh = false)
        {
            try
            {
                _logger.LogInformation("开始加载预警数据，强制刷新: {ForceRefresh}", forceRefresh);

                if (forceRefresh)
                {
                    // 如果是强制刷新，先清除可能的内存缓存
                    CurrentAlerts = new List<InventoryAlertResult>();
                    _logger.LogInformation("已清除内存中的预警数据");
                }

                // 调用领域服务获取预警数据
                CurrentAlerts = await _alertRuleDomain.CheckInventoryAlertsAsync(GetCurrentTenantId());
                _logger.LogInformation("预警数据加载完成，共 {Count} 个预警", CurrentAlerts.Count);

                // 记录详细的预警信息
                if (CurrentAlerts.Any())
                {
                    foreach (var alert in CurrentAlerts.Take(5)) // 只记录前5个避免日志过大
                    {
                        _logger.LogDebug("预警详情 - 药材: {HerbName}, 类型: {AlertType}, 优先级: {Priority}, 消息: {Message}",
                            alert.HerbName, alert.AlertType, alert.Priority, alert.AlertMessage);
                    }
                }
                else
                {
                    _logger.LogInformation("当前无预警信息");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载预警失败");
                CurrentAlerts = new List<InventoryAlertResult>();
                TempData["ErrorMessage"] = $"加载预警信息失败: {ex.Message}";
            }
        }

        // 获取预警优先级徽章样式
        public string GetAlertPriorityBadge(int priority)
        {
            return priority switch
            {
                3 => "bg-danger",
                2 => "bg-warning",
                1 => "bg-info",
                _ => "bg-secondary"
            };
        }

        // 获取优先级文本
        public string GetPriorityText(int priority)
        {
            return priority switch
            {
                3 => "紧急",
                2 => "高级",
                1 => "中级",
                _ => "普通"
            };
        }

        // 根据预警类型获取徽章样式
        public string GetAlertTypeBadge(string alertType)
        {
            return alertType switch
            {
                "LowStock" => "bg-danger",
                "Expiring" => "bg-warning",
                "Overstock" => "bg-info",
                _ => "bg-secondary"
            };
        }

        // 获取预警类型文本
        public string GetAlertTypeText(string alertType)
        {
            return alertType switch
            {
                "LowStock" => "低库存",
                "Expiring" => "临期预警",
                "Overstock" => "库存积压",
                _ => alertType
            };
        }

        // 获取预警图标
        public string GetAlertIcon(string alertType)
        {
            return alertType switch
            {
                "LowStock" => "fas fa-box",
                "Expiring" => "fas fa-clock",
                "Overstock" => "fas fa-warehouse",
                _ => "fas fa-bell"
            };
        }

        // 获取租户ID（从用户会话获取）
        private int GetCurrentTenantId()
        {
            // 这里应该从 HttpContext.User 获取租户ID
            // 简化实现，实际应从认证信息获取
            var tenantIdClaim = User.FindFirst("TenantId");
            return tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out int tenantId) ? tenantId : 1;
        }

        // 获取用户ID（从用户会话获取）
        private int GetCurrentUserId()
        {
            // 这里应该从 HttpContext.User 获取用户ID
            // 简化实现，实际应从认证信息获取
            var userIdClaim = User.FindFirst("UserId");
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 1;
        }
    }
}