using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Pages.Inventory
{
    public class EditModel : PageModel
    {
        private readonly InventoryManagementDomain _inventoryDomain;
        private readonly InventoryAlertRuleDomain _alertRuleDomain;
        private readonly TcmAiDiagnosisContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            InventoryManagementDomain inventoryDomain,
            InventoryAlertRuleDomain alertRuleDomain,
            TcmAiDiagnosisContext context,
            ILogger<EditModel> logger)
        {
            _inventoryDomain = inventoryDomain;
            _alertRuleDomain = alertRuleDomain;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InventoryItem InventoryItem { get; set; } = new();

        public List<Entities.Herb> Herbs { get; set; } = new();
        public List<InventoryOperation> RecentOperations { get; set; } = new();
        public List<InventoryAlertResult> ActiveAlerts { get; set; } = new();
        public int HealthScore { get; set; } = 100;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                await LoadInventoryItem(id);
                await LoadHerbs();
                await LoadRecentOperations(id);
                await LoadActiveAlerts(id);
                CalculateHealthScore();

                if (InventoryItem == null || InventoryItem.Id == 0)
                {
                    TempData["ErrorMessage"] = "未找到指定的库存项";
                    return RedirectToPage("/Inventory/Index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载库存编辑页面失败");
                TempData["ErrorMessage"] = "加载库存项失败";
                return RedirectToPage("/Inventory/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadHerbs();
                    await LoadRecentOperations(InventoryItem.Id);
                    await LoadActiveAlerts(InventoryItem.Id);
                    CalculateHealthScore();
                    return Page();
                }

                // 验证批次号唯一性（排除当前项）
                var existingBatch = await _context.InventoryItems
                    .AnyAsync(i => i.BatchNumber == InventoryItem.BatchNumber &&
                                  i.Id != InventoryItem.Id &&
                                  i.TenantId == InventoryItem.TenantId);

                if (existingBatch)
                {
                    ModelState.AddModelError("InventoryItem.BatchNumber", "批次号已存在");
                    await LoadHerbs();
                    await LoadRecentOperations(InventoryItem.Id);
                    await LoadActiveAlerts(InventoryItem.Id);
                    CalculateHealthScore();
                    return Page();
                }

                // 更新库存项
                await _inventoryDomain.UpdateInventoryItemAsync(InventoryItem);

                TempData["SuccessMessage"] = "库存项更新成功";
                return RedirectToPage("/Inventory/Details", new { id = InventoryItem.Id });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "更新库存项时未找到指定项");
                TempData["ErrorMessage"] = "未找到指定的库存项";
                return RedirectToPage("/Inventory/Index");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "更新库存项时发生业务错误");
                ModelState.AddModelError("", ex.Message);
                await LoadHerbs();
                await LoadRecentOperations(InventoryItem.Id);
                await LoadActiveAlerts(InventoryItem.Id);
                CalculateHealthScore();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新库存项失败");
                TempData["ErrorMessage"] = "更新库存项时发生错误";
                return RedirectToPage("/Inventory/Index");
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id)
        {
            try
            {
                await _inventoryDomain.BatchUpdateInventoryStatusAsync(InventoryItem.TenantId);
                TempData["SuccessMessage"] = "库存状态更新成功";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新库存状态失败");
                TempData["ErrorMessage"] = "更新库存状态失败";
            }

            return RedirectToPage(new { id });
        }

        private async Task LoadInventoryItem(int id)
        {
            var item = await _inventoryDomain.GetInventoryItemByIdAsync(id, true);
            if (item != null)
            {
                InventoryItem = item;
            }
            else
            {
                throw new KeyNotFoundException($"未找到ID为 {id} 的库存项");
            }
        }

        private async Task LoadHerbs()
        {
            Herbs = await _context.Herbs
                .Where(h => h.IsDeleted)
                .OrderBy(h => h.Name)
                .ToListAsync();
        }

        private async Task LoadRecentOperations(int inventoryItemId)
        {
            RecentOperations = await _context.InventoryOperations
                .Include(o => o.Operator)
                .Where(o => o.InventoryItemId == inventoryItemId)
                .OrderByDescending(o => o.OperatedAt)
                .Take(10)
                .ToListAsync();
        }

        private async Task LoadActiveAlerts(int inventoryItemId)
        {
            try
            {
                var allAlerts = await _alertRuleDomain.CheckInventoryAlertsAsync(InventoryItem.TenantId);
                ActiveAlerts = allAlerts
                    .Where(a => a.InventoryItemId == inventoryItemId)
                    .OrderByDescending(a => a.Priority)
                    .ThenByDescending(a => a.TriggeredAt)
                    .Take(5)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "加载库存预警失败");
                ActiveAlerts = new List<InventoryAlertResult>();
            }
        }

        private void CalculateHealthScore()
        {
            if (InventoryItem == null)
            {
                HealthScore = 0;
                return;
            }

            var score = 100;

            // 根据库存状态扣分
            switch (InventoryItem.Status)
            {
                case "Expired":
                    score -= 50;
                    break;
                case "Expiring":
                    score -= 30;
                    break;
                case "LowStock":
                    score -= 20;
                    break;
                case "OutOfStock":
                    score -= 40;
                    break;
            }

            // 根据质量状态扣分
            if (InventoryItem.QualityStatus == "Unqualified")
            {
                score -= 30;
            }
            else if (InventoryItem.QualityStatus == "Pending")
            {
                score -= 10;
            }

            // 根据库存比例调整
            if (InventoryItem.InitialQuantity > 0)
            {
                var ratio = InventoryItem.CurrentQuantity / InventoryItem.InitialQuantity;
                if (ratio < 0.1m)
                {
                    score -= 15;
                }
                else if (ratio < 0.3m)
                {
                    score -= 5;
                }
            }

            HealthScore = Math.Max(0, Math.Min(100, score));
        }

        public decimal GetTotalValue()
        {
            return (InventoryItem?.CurrentQuantity ?? 0) * (InventoryItem?.PurchasePrice ?? 0);
        }
    }
}