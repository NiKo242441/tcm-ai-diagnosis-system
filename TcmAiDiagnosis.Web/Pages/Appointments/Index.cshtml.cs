using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Pages.Appointments
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly TcmAiDiagnosisContext _context;

        public IndexModel(TcmAiDiagnosisContext context)
        {
            _context = context;
        }

        public List<Appointment> Appointments { get; set; } = new();

        //public async Task OnGetAsync()
        //{
        //    try
        //    {
        //        var response = await _httpClient.GetFromJsonAsync<List<Appointment>>("/api/Appointments");
        //        if (response != null)
        //            Appointments = response;
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        // HttpRequestException 没有 Response 属性，无法直接获取响应内容
        //        // 可以考虑记录异常消息
        //        Console.WriteLine($"API 错误: {ex.Message}");
        //        throw; // 重新抛出异常
        //    }
        //    //var response = await _httpClient.GetFromJsonAsync<List<Appointment>>("/api/Appointments");
        //}
        public async Task OnGetAsync()
        {
            //try
            //{
            //    //var response = await _httpClient.GetAsync("/api/Appointments");
            //    var response = await _httpClient.GetAsync("http://localhost:5226/api/Appointments");
            //    if (response.IsSuccessStatusCode)
            //    {
            //        var appointments = await response.Content.ReadFromJsonAsync<List<Appointment>>();
            //        if (appointments != null)
            //            Appointments = appointments;
            //    }
            //    else
            //    {
            //        // 记录错误信息
            //        var errorContent = await response.Content.ReadAsStringAsync();
            //        Console.WriteLine($"API 错误: {response.StatusCode}, 内容: {errorContent}");
            //    }
            //}
            //catch (HttpRequestException ex)
            //{
            //    Console.WriteLine($"请求异常: {ex.Message}");
            //    throw;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"未知异常: {ex.Message}");
            //    throw;
            //}
            Appointments = await _context.Appointments
            .OrderByDescending(a => a.AppointmentTime)
            .ToListAsync();
        }
    }
}
