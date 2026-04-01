using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.Dtos;

namespace TcmAiDiagnosis.Web.Pages.HerbContraindication
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // 搜索条件
        [BindProperty(SupportsGet = true)]
        public string SearchPrimaryHerb { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string SearchConflictHerb { get; set; } = string.Empty;

        // 分页数据
        public PagedResult<HerbContraindicationDto> Contraindications { get; set; } = new();

        // 当前页
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        // 页面大小
        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync()
        {
            // 构造查询参数
            var queryParams = new Dictionary<string, string>
    {
        { "PageNumber", PageNumber.ToString() },
        { "PageSize", PageSize.ToString() },
        { "SearchPrimaryHerb", SearchPrimaryHerb ?? string.Empty },
        { "SearchConflictHerb", SearchConflictHerb ?? string.Empty }
    };

            var queryString = QueryString(queryParams);

            // 使用绝对 URI 调用 API
            var apiUrl = $"http://localhost:5226/api/HerbContraindication/query{queryString}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                Contraindications = await response.Content.ReadFromJsonAsync<PagedResult<HerbContraindicationDto>>()
                                     ?? new PagedResult<HerbContraindicationDto>();
            }
        }


        // 构造查询字符串
        private string QueryString(Dictionary<string, string> parameters)
        {
            var array = new List<string>();
            foreach (var kv in parameters)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    array.Add($"{kv.Key}={System.Net.WebUtility.UrlEncode(kv.Value)}");
                }
            }
            return array.Count > 0 ? "?" + string.Join("&", array) : string.Empty;
        }
    }
}
