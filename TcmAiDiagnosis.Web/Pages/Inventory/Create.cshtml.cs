using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Pages.Inventory
{
    public class CreateModel : PageModel
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(TcmAiDiagnosisContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public CreateInventoryItemDto Input { get; set; } = new CreateInventoryItemDto();

        public List<Entities.Herb> Herbs { get; set; } = new List<Entities.Herb>();
        public int TenantId { get; set; } = 1;

        public async Task OnGetAsync()
        {
            await LoadHerbsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadHerbsAsync();
                return Page();
            }

            try
            {
                // 检查批次号是否重复
                var exists = await _context.InventoryItems
                    .AnyAsync(i => i.BatchNumber == Input.BatchNumber && i.TenantId == TenantId);
                if (exists)
                {
                    ModelState.AddModelError(string.Empty, $"批次号「{Input.BatchNumber}」已存在");
                    await LoadHerbsAsync();
                    return Page();
                }

                // 创建库存项实体
                var item = new InventoryItem
                {
                    BatchNumber = Input.BatchNumber,
                    HerbId = Input.HerbId,
                    CurrentQuantity = Input.CurrentQuantity,
                    InitialQuantity = Input.CurrentQuantity,
                    Unit = Input.Unit,
                    StorageLocation = Input.StorageLocation,
                    PurchasePrice = Input.PurchasePrice,
                    SalePrice = Input.SalePrice,
                    ProductionDate = Input.ProductionDate,
                    ExpiryDate = Input.ExpiryDate,
                    SupplierName = Input.SupplierName,
                    QualityStatus = Input.QualityStatus,
                    Remark = Input.Remark,
                    TenantId = TenantId,
                    CreatedBy = 1, // 从用户信息获取
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true,
                    Status = "Normal"
                };

                _context.InventoryItems.Add(item);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "库存添加成功";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "数据库保存失败");
                ModelState.AddModelError(string.Empty, "保存数据时发生错误，请检查数据有效性");
                await LoadHerbsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加库存时发生错误");
                ModelState.AddModelError(string.Empty, "添加库存时发生错误，请重试");
                await LoadHerbsAsync();
                return Page();
            }
        }

        private async Task LoadHerbsAsync()
        {
            try
            {
                // 修复：移除 IsActive 条件，因为实体中没有这个属性
                var herbs = await _context.Herbs
                    .Where(h => !h.IsDeleted) // 只获取未删除的药材
                    .OrderBy(h => h.Name)
                    .ToListAsync();

                Herbs = herbs ?? new List<Entities.Herb>();

                _logger.LogInformation($"成功加载 {Herbs.Count} 个药材");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载药材列表时发生错误");
                Herbs = new List<Entities.Herb>();
            }
        }
    }
}