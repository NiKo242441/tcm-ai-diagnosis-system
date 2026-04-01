using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Pages.Herb
{
    public class EditModel : PageModel
    {
        private readonly HerbDomain _herbDomain;
        private readonly ILogger<EditModel> _logger;

        public EditModel(HerbDomain herbDomain, ILogger<EditModel> logger)
        {
            _herbDomain = herbDomain;
            _logger = logger;
        }

        [BindProperty]
        public UpdateHerbDto Herb { get; set; } = new UpdateHerbDto();

        public List<string>? Categories { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // 获取药材信息
                var herbEntity = await _herbDomain.GetHerbByIdAsync(id);
                if (herbEntity == null)
                {
                    _logger.LogWarning($"未找到要编辑的药材ID: {id}");
                    return NotFound();
                }

                // 转换为 DTO - 修正属性映射
                Herb = new UpdateHerbDto
                {
                    HerbId = herbEntity.Id, // 修正：Id -> HerbId
                    HerbName = herbEntity.Name, // 修正：Name -> HerbName
                    Properties = herbEntity.Properties,
                    Meridians = herbEntity.Meridians,
                    Efficacy = herbEntity.Efficacy,
                    Category = herbEntity.Category,
                    Dosage = herbEntity.DosageRange, // 修正：DosageRange -> Dosage
                    Precautions = herbEntity.Precautions,
                    ProcessingMethod = herbEntity.ProcessingMethods, // 修正：ProcessingMethods -> ProcessingMethod
                    ToxicityLevel = herbEntity.ToxicityLevel,
                    IsActive = !herbEntity.IsDeleted // 修正：IsDeleted -> IsActive (逻辑反转)
                };

                // 获取分类列表
                Categories = await _herbDomain.GetHerbCategoriesAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载药材编辑页面失败，药材ID: {id}");
                TempData["ErrorMessage"] = "加载编辑页面时发生错误";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"模型验证失败: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");

                // 重新加载分类列表
                Categories = await _herbDomain.GetHerbCategoriesAsync();
                return Page();
            }

            try
            {
                // 获取现有药材信息
                var existingHerb = await _herbDomain.GetHerbByIdAsync(Herb.HerbId);
                if (existingHerb == null)
                {
                    _logger.LogWarning($"未找到要更新的药材ID: {Herb.HerbId}");
                    TempData["ErrorMessage"] = "未找到指定药材";
                    Categories = await _herbDomain.GetHerbCategoriesAsync();
                    return Page();
                }

                // 更新字段 - 修正属性映射
                existingHerb.Name = Herb.HerbName; // 修正：HerbName -> Name
                existingHerb.Properties = Herb.Properties ?? string.Empty;
                existingHerb.Meridians = Herb.Meridians ?? string.Empty;
                existingHerb.Efficacy = Herb.Efficacy ?? string.Empty;
                existingHerb.Category = Herb.Category ?? string.Empty;
                existingHerb.DosageRange = Herb.Dosage ?? string.Empty; // 修正：Dosage -> DosageRange
                existingHerb.Precautions = Herb.Precautions ?? string.Empty;
                existingHerb.ProcessingMethods = Herb.ProcessingMethod ?? string.Empty; // 修正：ProcessingMethod -> ProcessingMethods
                existingHerb.ToxicityLevel = Herb.ToxicityLevel ?? "无毒";
                existingHerb.UpdatedAt = DateTime.UtcNow;
                existingHerb.IsDeleted = !Herb.IsActive; // 修正：IsActive -> IsDeleted (逻辑反转)

                _logger.LogInformation($"更新药材信息: ID={Herb.HerbId}, Name={Herb.HerbName}");

                // 调用领域服务更新药材
                await _herbDomain.UpdateHerbAsync(existingHerb);

                _logger.LogInformation($"药材更新成功: ID={Herb.HerbId}");
                TempData["SuccessMessage"] = "药材信息更新成功";
                return RedirectToPage("./Details", new { id = Herb.HerbId });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"更新药材时未找到: {Herb.HerbId}");
                TempData["ErrorMessage"] = "未找到指定药材";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"业务逻辑错误: {ex.Message}");
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新药材失败，药材ID: {Herb.HerbId}");
                TempData["ErrorMessage"] = "更新药材信息时发生错误";
            }

            // 如果出错，重新加载分类列表
            Categories = await _herbDomain.GetHerbCategoriesAsync();
            return Page();
        }
    }
}