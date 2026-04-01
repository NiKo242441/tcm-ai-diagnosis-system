using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.EFContext;
using Microsoft.EntityFrameworkCore;

namespace TcmAiDiagnosis.Pages.Inventory
{
    public class IndexModel : PageModel
    {
        private readonly InventoryManagementDomain _inventoryDomain;
        private readonly TcmAiDiagnosisContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(InventoryManagementDomain inventoryDomain, TcmAiDiagnosisContext context, ILogger<IndexModel> logger)
        {
            _inventoryDomain = inventoryDomain;
            _context = context;
            _logger = logger;
        }

        public PagedResult<InventoryItem>? InventoryItems { get; set; }
        public List<Entities.Herb> Herbs { get; set; } = new List<Entities.Herb>();
        public InventorySummary? Summary { get; set; }
        public string? SearchKeyword { get; set; }
        public int? SelectedHerbId { get; set; }
        public string? SelectedStatus { get; set; }
        public string? SelectedQualityStatus { get; set; }

        public async Task OnGetAsync(string? searchKeyword, int? herbId, string? status,
                                   string? qualityStatus, int pageNumber = 1, int pageSize = 10)
        {
            SearchKeyword = searchKeyword;
            SelectedHerbId = herbId;
            SelectedStatus = status;
            SelectedQualityStatus = qualityStatus;

            try
            {
                // 加载库存列表
                await LoadInventoryItems(pageNumber, pageSize);

                // 加载药材列表
                await LoadHerbs();

                // 加载库存统计
                await LoadSummary();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载库存页面失败");
                InventoryItems = new PagedResult<InventoryItem> { Items = new List<InventoryItem>() };
            }
        }

        private async Task LoadInventoryItems(int pageNumber, int pageSize)
        {
            try
            {
                var request = new PagedRequest
                {
                    Page = pageNumber,
                    PageSize = pageSize,
                    SearchKeyword = SearchKeyword
                };

                InventoryItems = await _inventoryDomain.GetPagedInventoryItemsAsync(
                    request,
                    tenantId: 1, // 从用户信息获取
                    herbId: SelectedHerbId,
                    status: SelectedStatus,
                    qualityStatus: SelectedQualityStatus
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载库存列表失败");
                InventoryItems = new PagedResult<InventoryItem> { Items = new List<InventoryItem>() };
            }
        }

        private async Task LoadHerbs()
        {
            try
            {
                Herbs = await _context.Herbs
                    .Where(h => h.IsDeleted)
                    .OrderBy(h => h.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载药材列表失败");
                Herbs = new List<Entities.Herb>();
            }
        }

        private async Task LoadSummary()
        {
            try
            {
                _logger.LogInformation("开始加载库存统计...");
                Summary = await _inventoryDomain.GetInventorySummaryAsync(tenantId: 1);

                if (Summary == null)
                {
                    _logger.LogWarning("库存统计返回为null，创建默认Summary");
                    Summary = new InventorySummary();
                }

                _logger.LogInformation($"库存统计加载完成: TotalItems={Summary.TotalItems}, LowStockCount={Summary.LowStockCount}, ExpiringCount={Summary.ExpiringCount}, ExpiredCount={Summary.ExpiredCount}, OutOfStockCount={Summary.OutOfStockCount}, TotalHerbTypes={Summary.TotalHerbTypes}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载库存统计失败");
                Summary = new InventorySummary
                {
                    TotalItems = 0,
                    LowStockCount = 0,
                    ExpiringCount = 0,
                    ExpiredCount = 0,
                    OutOfStockCount = 0,
                    TotalHerbTypes = 0
                };
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _inventoryDomain.DeleteInventoryItemAsync(id);
                TempData["SuccessMessage"] = "库存项删除成功";
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "删除库存项时未找到指定项");
                TempData["ErrorMessage"] = "未找到指定的库存项";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除库存项失败");
                TempData["ErrorMessage"] = "删除库存项时发生错误";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id, bool isActive)
        {
            try
            {
                await _inventoryDomain.ToggleInventoryItemAsync(id, isActive);
                TempData["SuccessMessage"] = isActive ? "库存项已启用" : "库存项已禁用";
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "切换库存项状态时未找到指定项");
                TempData["ErrorMessage"] = "未找到指定的库存项";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切换库存项状态失败");
                TempData["ErrorMessage"] = "切换库存项状态时发生错误";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostOperationAsync(int inventoryItemId, string operationType,
                                                            decimal quantity, string? referenceNumber, string? reason)
        {
            try
            {
                var operatedBy = 1; // 这里应该从登录用户信息获取

                switch (operationType)
                {
                    case "PurchaseIn":
                        await _inventoryDomain.PurchaseInAsync(inventoryItemId, quantity, referenceNumber ?? "", operatedBy, reason ?? "");
                        break;
                    case "SaleOut":
                        await _inventoryDomain.SaleOutAsync(inventoryItemId, quantity, referenceNumber ?? "", operatedBy, reason ?? "");
                        break;
                    case "Adjust":
                        await _inventoryDomain.AdjustInventoryAsync(inventoryItemId, quantity, operatedBy, reason ?? "");
                        break;
                    case "Loss":
                        await _inventoryDomain.LossInventoryAsync(inventoryItemId, quantity, operatedBy, reason ?? "");
                        break;
                    case "Check":
                        // 盘点操作需要实际数量
                        await _inventoryDomain.CheckInventoryAsync(inventoryItemId, quantity, operatedBy, reason ?? "");
                        break;
                    default:
                        TempData["ErrorMessage"] = "未知的操作类型";
                        return RedirectToPage();
                }

                TempData["SuccessMessage"] = "库存操作成功";
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "库存操作时未找到指定项");
                TempData["ErrorMessage"] = "未找到指定的库存项";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "库存操作失败");
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "库存操作失败");
                TempData["ErrorMessage"] = "库存操作时发生错误";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBatchUpdateStatusAsync()
        {
            try
            {
                await _inventoryDomain.BatchUpdateInventoryStatusAsync(tenantId: 1);
                TempData["SuccessMessage"] = "库存状态批量更新成功";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新库存状态失败");
                TempData["ErrorMessage"] = "批量更新库存状态时发生错误";
            }

            return RedirectToPage();
        }

        // 获取低库存项 - 修正版本
        public async Task<IActionResult> OnGetLowStockAsync()
        {
            try
            {
                // 设置筛选条件为低库存
                SelectedStatus = "LowStock";
                SearchKeyword = null;
                SelectedHerbId = null;
                SelectedQualityStatus = null;

                // 重新加载数据
                await LoadInventoryItems(1, 10);
                await LoadHerbs();
                await LoadSummary();

                var lowStockItems = await _inventoryDomain.GetLowStockItemsAsync(tenantId: 1);

                if (lowStockItems.Count > 0)
                {
                    TempData["InfoMessage"] = $"已筛选显示 {lowStockItems.Count} 个低库存项";
                }
                else
                {
                    TempData["SuccessMessage"] = "当前没有低库存项";
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取低库存项失败");
                TempData["ErrorMessage"] = "获取低库存项时发生错误";
                return RedirectToPage();
            }
        }

        // 获取临期项 - 修正版本
        public async Task<IActionResult> OnGetExpiringAsync()
        {
            try
            {
                // 设置筛选条件为临期
                SelectedStatus = "Expiring";
                SearchKeyword = null;
                SelectedHerbId = null;
                SelectedQualityStatus = null;

                // 重新加载数据
                await LoadInventoryItems(1, 10);
                await LoadHerbs();
                await LoadSummary();

                var expiringItems = await _inventoryDomain.GetExpiringItemsAsync(tenantId: 1);

                if (expiringItems.Count > 0)
                {
                    TempData["WarningMessage"] = $"已筛选显示 {expiringItems.Count} 个临期项，请及时处理";
                }
                else
                {
                    TempData["SuccessMessage"] = "当前没有临期项";
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取临期项失败");
                TempData["ErrorMessage"] = "获取临期项时发生错误";
                return RedirectToPage();
            }
        }

        // 获取过期项 - 修正版本
        public async Task<IActionResult> OnGetExpiredAsync()
        {
            try
            {
                // 设置筛选条件为过期
                SelectedStatus = "Expired";
                SearchKeyword = null;
                SelectedHerbId = null;
                SelectedQualityStatus = null;

                // 重新加载数据
                await LoadInventoryItems(1, 10);
                await LoadHerbs();
                await LoadSummary();

                var expiredItems = await _inventoryDomain.GetExpiredItemsAsync(tenantId: 1);

                if (expiredItems.Count > 0)
                {
                    TempData["DangerMessage"] = $"已筛选显示 {expiredItems.Count} 个过期项，请立即处理";
                }
                else
                {
                    TempData["SuccessMessage"] = "当前没有过期项";
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取过期项失败");
                TempData["ErrorMessage"] = "获取过期项时发生错误";
                return RedirectToPage();
            }
        }

        // 新增：处理AJAX操作请求
        public async Task<JsonResult> OnPostOperation()
        {
            try
            {
                var inventoryItemId = int.Parse(Request.Form["inventoryItemId"]);
                var operationType = Request.Form["operationType"];
                var quantity = decimal.Parse(Request.Form["quantity"]);
                var referenceNumber = Request.Form["referenceNumber"];
                var reason = Request.Form["reason"];
                var operatedBy = 1; // 从用户信息获取

                switch (operationType)
                {
                    case "PurchaseIn":
                        await _inventoryDomain.PurchaseInAsync(inventoryItemId, quantity, referenceNumber, operatedBy, reason);
                        break;
                    case "SaleOut":
                        await _inventoryDomain.SaleOutAsync(inventoryItemId, quantity, referenceNumber, operatedBy, reason);
                        break;
                    case "Adjust":
                        await _inventoryDomain.AdjustInventoryAsync(inventoryItemId, quantity, operatedBy, reason);
                        break;
                    default:
                        return new JsonResult(new { success = false, message = "未知的操作类型" });
                }

                return new JsonResult(new { success = true, message = "库存操作成功" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "库存操作时未找到指定项");
                return new JsonResult(new { success = false, message = "未找到指定的库存项" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "库存操作失败");
                return new JsonResult(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "库存操作失败");
                return new JsonResult(new { success = false, message = "库存操作时发生错误" });
            }
        }
    }
}