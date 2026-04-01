using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Pages.Herb
{
    public class IndexModel : PageModel
    {
        private readonly HerbDomain _herbDomain;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(HerbDomain herbDomain, ILogger<IndexModel> logger)
        {
            _herbDomain = herbDomain;
            _logger = logger;
        }

        public PagedResult<HerbDto>? Herbs { get; set; }
        public List<string>? Categories { get; set; }
        public string? SearchKeyword { get; set; }
        public string? SelectedCategory { get; set; }
        public int? SelectedToxicity { get; set; } // 改回 int?

        public async Task OnGetAsync(string? searchKeyword, string? category, int? toxicity, int pageNumber = 1, int pageSize = 10) // toxicity 改回 int?
        {
            SearchKeyword = searchKeyword;
            SelectedCategory = category;
            SelectedToxicity = toxicity; // 现在类型匹配

            try
            {
                // 创建分页请求
                var request = new PagedRequest
                {
                    Page = pageNumber,
                    PageSize = pageSize,
                    SearchKeyword = searchKeyword
                };

                // 直接调用领域服务获取分页数据 - 现在参数类型匹配
                var result = await _herbDomain.GetPagedHerbsAsync(request, category, toxicity);

                // 将实体映射为DTO
                Herbs = new PagedResult<HerbDto>
                {
                    Items = result.Items.Select(h => MapToDto(h)).ToList(),
                    TotalCount = result.TotalCount,
                    Page = result.Page,
                    PageSize = result.PageSize
                };

                _logger.LogInformation($"成功加载 {Herbs.Items.Count} 条药材记录");

                // 直接调用领域服务获取分类列表
                Categories = await _herbDomain.GetHerbCategoriesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载药材列表失败");
                Herbs = new PagedResult<HerbDto> { Items = new List<HerbDto>() };
                Categories = new List<string>();
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
                    return RedirectToPage();
                }

                // 执行软删除（设置IsDeleted为true）
                herb.IsDeleted = true;
                herb.UpdatedAt = DateTime.UtcNow;
                await _herbDomain.UpdateHerbAsync(herb);

                TempData["SuccessMessage"] = "药材删除成功";
                _logger.LogInformation($"成功删除药材ID: {id}");
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

            return RedirectToPage();
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
                IsActive = !herb.IsDeleted, // 修正：IsDeleted -> IsActive (逻辑反转)
                CreatedAt = herb.CreatedAt,
                UpdatedAt = herb.UpdatedAt ?? herb.CreatedAt,
                // 以下字段在Herb实体中不存在，设置为null
                Source = null,
                Remarks = null
            };
        }
    }
}