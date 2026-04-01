using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Pages.Herb
{
    public class CreateModel : PageModel
    {
        private readonly HerbDomain _herbDomain;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(HerbDomain herbDomain, ILogger<CreateModel> logger)
        {
            _herbDomain = herbDomain;
            _logger = logger;
        }

        [BindProperty]
        public CreateHerbDto Herb { get; set; } = new CreateHerbDto();

        public List<string>? Categories { get; set; }

        public async Task OnGetAsync()
        {
            await LoadCategories();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"模型验证失败: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                await LoadCategories();
                return Page();
            }

            try
            {
                // 将 DTO 转换为实体 - 修正属性映射
                var herbEntity = new Entities.Herb
                {
                    Name = Herb.HerbName, // 修正：HerbName -> Name
                    Properties = Herb.Properties ?? string.Empty,
                    Meridians = Herb.Meridians ?? string.Empty,
                    Efficacy = Herb.Efficacy ?? string.Empty,
                    Category = Herb.Category ?? string.Empty,
                    DosageRange = Herb.Dosage ?? string.Empty, // 修正：Dosage -> DosageRange
                    Precautions = Herb.Precautions ?? string.Empty,
                    ProcessingMethods = Herb.ProcessingMethod ?? string.Empty, // 修正：ProcessingMethod -> ProcessingMethods
                    ToxicityLevel = Herb.ToxicityLevel ?? "无毒",
                    // 设置默认值
                    Aliases = string.Empty,
                    CommonUnit = "g",
                    Contraindications = string.Empty,
                    IsCommonlyUsed = false,
                    RequiresSpecialHandling = false,
                    SpecialUsageInstructions = string.Empty,
                    IsAiOriginated = false,
                    Status = ReviewStatus.AIGenerated,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false, // 新创建默认未删除
                    TenantId = 1 // 设置默认租户ID
                };

                _logger.LogInformation($"创建药材: {herbEntity.Name}");

                // 直接调用领域服务创建药材
                var createdHerb = await _herbDomain.AddHerbAsync(herbEntity);

                _logger.LogInformation($"药材创建成功，ID: {createdHerb.Id}"); // 修正：HerbId -> Id
                TempData["SuccessMessage"] = "药材创建成功";
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"参数错误: {ex.Message}");
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadCategories();
                return Page();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"业务逻辑错误: {ex.Message}");
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadCategories();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建药材失败");
                ModelState.AddModelError(string.Empty, $"创建药材时发生错误: {ex.Message}");
                await LoadCategories();
                return Page();
            }
        }

        private async Task LoadCategories()
        {
            try
            {
                // 直接调用领域服务获取分类列表
                Categories = await _herbDomain.GetHerbCategoriesAsync();
                _logger.LogInformation($"加载了 {Categories?.Count ?? 0} 个分类");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载分类列表失败");
                Categories = new List<string>();
            }
        }
    }
}