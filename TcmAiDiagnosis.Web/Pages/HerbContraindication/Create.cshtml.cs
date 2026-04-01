using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Dtos;
using System.Net.Http.Json;

namespace TcmAiDiagnosis.Web.Pages.HerbContraindication
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        // 药材列表，用于下拉选择
        public List<HerbDto> Herbs { get; set; } = new();

        // 表单绑定
        [BindProperty]
        public CreateHerbContraindicationDto NewContraindication { get; set; } = new();

        // 页面初始加载
        public async Task OnGetAsync()
        {
            Herbs = await _httpClient.GetFromJsonAsync<List<HerbDto>>("/api/Herb") ?? new List<HerbDto>();
        }

        // 表单提交
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // 保持下拉列表
                return Page();
            }

            var response = await _httpClient.PostAsJsonAsync("/api/HerbContraindication", NewContraindication);

            if (response.IsSuccessStatusCode)
            {
                // 提交成功，跳转回列表页
                return RedirectToPage("Index");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, error);
                await OnGetAsync();
                return Page();
            }
        }
    }
}
