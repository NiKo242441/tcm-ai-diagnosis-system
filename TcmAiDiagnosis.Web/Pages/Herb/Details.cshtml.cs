using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Pages.Herb
{
    public class DetailsModel : PageModel
    {
        private readonly HerbDomain _herbDomain;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(HerbDomain herbDomain, ILogger<DetailsModel> logger)
        {
            _herbDomain = herbDomain;
            _logger = logger;
        }

        public HerbDto? Herb { get; set; }
        public List<HerbDto>? ContraindicatedHerbs { get; set; }
        public bool HasContraindications { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // 获取药材基本信息（包含配伍禁忌信息）
                var herbEntity = await _herbDomain.GetHerbByIdAsync(id, includeContraindications: true);
                if (herbEntity == null)
                {
                    _logger.LogWarning($"未找到药材ID: {id}");
                    return NotFound();
                }

                // 转换为 DTO
                Herb = MapToDto(herbEntity);

                // 检查是否存在配伍禁忌
                HasContraindications = await _herbDomain.HasContraindicationsAsync(id);

                // 获取配伍禁忌药材列表
                if (HasContraindications)
                {
                    var contraindicatedHerbEntities = await _herbDomain.GetContraindicatedHerbsAsync(id);
                    ContraindicatedHerbs = contraindicatedHerbEntities.Select(h => MapToDto(h)).ToList();
                    _logger.LogInformation($"获取到 {ContraindicatedHerbs.Count} 个配伍禁忌药材");
                }

                _logger.LogInformation($"成功加载药材详情: {Herb.HerbName}");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载药材详情失败，药材ID: {id}");
                TempData["ErrorMessage"] = "加载药材详情时发生错误";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                // 直接调用领域服务进行软删除
                var herb = await _herbDomain.GetHerbByIdAsync(id);
                if (herb == null)
                {
                    TempData["ErrorMessage"] = "未找到指定药材";
                    return RedirectToPage(new { id });
                }

                // 执行软删除（设置IsDeleted为true）
                herb.IsDeleted = true;
                herb.UpdatedAt = DateTime.UtcNow;
                await _herbDomain.UpdateHerbAsync(herb);

                TempData["SuccessMessage"] = "药材删除成功";
                _logger.LogInformation($"成功删除药材ID: {id}");
                return RedirectToPage("./Index");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"删除药材时未找到: {id}");
                TempData["ErrorMessage"] = "未找到指定药材";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除药材失败，药材ID: {id}");
                TempData["ErrorMessage"] = "删除药材时发生错误";
            }

            return RedirectToPage(new { id });
        }

        /// <summary>
        /// 将Herb实体映射为HerbDto - 修正属性映射
        /// </summary>
        private HerbDto MapToDto(Entities.Herb herb)
        {
            return new HerbDto
            {
                HerbId = herb.Id, // 修正：Id -> HerbId
                HerbName = herb.Name, // 修正：Name -> HerbName
                Properties = herb.Properties,
                Meridians = herb.Meridians,
                Efficacy = herb.Efficacy,
                Category = herb.Category,
                Dosage = herb.DosageRange, // 修正：DosageRange -> Dosage
                Precautions = herb.Precautions,
                ProcessingMethod = herb.ProcessingMethods, // 修正：ProcessingMethods -> ProcessingMethod
                ToxicityLevel = herb.ToxicityLevel,
                IsActive = !herb.IsDeleted, // 修正：IsDeleted -> IsActive
                CreatedAt = herb.CreatedAt,
                UpdatedAt = herb.UpdatedAt ?? herb.CreatedAt,
                // 以下字段在Herb实体中不存在，设置为null或默认值
                Source = null,
                Remarks = null
            };
        }
    }
}