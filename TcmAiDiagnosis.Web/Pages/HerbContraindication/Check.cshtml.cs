using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Dtos;

namespace TcmAiDiagnosis.Web.Pages.HerbContraindication
{
    public class CheckModel : PageModel
    {
        private readonly HerbDomain _herbDomain;

        public CheckModel(HerbDomain herbDomain)
        {
            _herbDomain = herbDomain;
        }

        [BindProperty]
        public List<int> SelectedHerbIds { get; set; } = new();

        public List<HerbDto> AllHerbs { get; set; } = new();

        public List<(HerbDto Herb1, HerbDto Herb2)> ConflictingHerbs { get; set; } = new();

        public async Task OnGetAsync()
        {
            // 获取所有药材，供前端选择
            var paged = await _herbDomain.GetPagedHerbsAsync(
                new Domain.Paged.PagedRequest { Page = 1, PageSize = 1000 },
                category: null,
                toxicityLevel: null);

            AllHerbs = paged.Items.Select(h => new HerbDto
            {
                HerbId = h.Id,
                HerbName = h.Name
            }).ToList();
        }

        public async Task<IActionResult> OnPostCheckAsync()
        {
            if (SelectedHerbIds == null || SelectedHerbIds.Count < 2)
            {
                ModelState.AddModelError("", "请至少选择两种药材进行检查");
                await OnGetAsync(); // 重新加载药材列表
                return Page();
            }

            // 假设 CheckHerbConflictsAsync 返回 List<(Herb HerbA, Herb HerbB)>
            var conflicts = await _herbDomain.CheckHerbConflictsAsync(SelectedHerbIds);

            ConflictingHerbs = conflicts.Select(c => (
                new HerbDto { HerbId = c.HerbA.Id, HerbName = c.HerbA.Name },
                new HerbDto { HerbId = c.HerbB.Id, HerbName = c.HerbB.Name }
            )).ToList();

            await OnGetAsync(); // 重新加载药材列表
            return Page();
        }
    }
}
