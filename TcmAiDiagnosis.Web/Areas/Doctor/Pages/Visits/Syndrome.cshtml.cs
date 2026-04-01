using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Areas.Doctor.Pages.Visits
{
    public class SyndromeModel : PageModel
    {
        private readonly SyndromeDomain _syndromeDomain;
        private readonly VisitDomain _visitDomain;
        private readonly PatientDomain _patientDomain;
        private readonly UserManager<User> _userManager;
        private readonly IServiceProvider _serviceProvider;

        public SyndromeModel(SyndromeDomain syndromeDomain, VisitDomain visitDomain, PatientDomain patientDomain, UserManager<User> userManager, IServiceProvider serviceProvider)
        {
            _syndromeDomain = syndromeDomain;
            _visitDomain = visitDomain;
            _patientDomain = patientDomain;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        [BindProperty(SupportsGet = true)]
        public int VisitId { get; set; }

        public List<Syndrome> Syndromes { get; set; } = new List<Syndrome>();
        public Visit? CurrentVisit { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (VisitId <= 0)
            {
                return BadRequest("就诊ID无效");
            }

            // 获取就诊信息
            CurrentVisit = await _visitDomain.GetVisitByIdAsync(VisitId);
            if (CurrentVisit == null)
            {
                return NotFound("未找到就诊记录");
            }

            // 检查是否已有证候数据
            Syndromes = await _syndromeDomain.GetSyndromesByVisitIdAsync(VisitId);

            // 如果没有证候数据，则调用AI进行诊断
            if (!Syndromes.Any())
            {
                try
                {
                    // 获取当前登录用户的手机号
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser == null)
                    {
                        return RedirectToPage("/Account/Login", new { area = "Identity" });
                    }
                    var userPhoneNumber = currentUser.PhoneNumber;

                    // 获取患者信息和问诊描述
                    var patientDescription = await _patientDomain.GetPatientNotes(CurrentVisit.PatientUserId);
                    var visitDescription = CurrentVisit.VisitNotes;

                    // 调用Dify API获取证候概览
                    var syndromeOverviews = await _syndromeDomain.GetSyndromeOverviewAsync(patientDescription, visitDescription, userPhoneNumber ?? "");

                    // 保存证候概览到数据库
                    Syndromes = await _syndromeDomain.SaveSyndromeOverviewAsync(VisitId, syndromeOverviews);

                    // 异步获取证候详情（不阻塞页面加载）
                    // 使用新的作用域来避免 DbContext 并发问题
                    _ = Task.Run(async () =>
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var scopedSyndromeDomain = scope.ServiceProvider.GetRequiredService<SyndromeDomain>();
                        await scopedSyndromeDomain.FetchSyndromeDetailsAsync(Syndromes, patientDescription, visitDescription, userPhoneNumber ?? "");
                    });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"AI诊断失败: {ex.Message}";
                }
            }

            return Page();
        }

        /// <summary>
        /// 获取证候详情
        /// </summary>
        /// <param name="syndromeId">证候ID</param>
        /// <returns>JSON格式的证候详情</returns>
        public async Task<IActionResult> OnGetSyndromeDetailAsync(int syndromeId)
        {
            var syndrome = await _syndromeDomain.GetSyndromeByIdAsync(syndromeId);
            if (syndrome == null)
            {
                return new JsonResult(new { success = false, message = "证候不存在" });
            }

            switch (syndrome.DetailStatus)
            {
                case 0: // 未获取详情
                    return new JsonResult(new { success = false, status = "pending", message = "正在获取详情，请稍候..." });
                case 1: // 正在获取详情
                    return new JsonResult(new { success = false, status = "loading", message = "AI正在分析中，请稍候..." });
                case 2: // 已获取详情
                    // 反序列化诊断信息
                    DiagnosisInfo? diagnosisInfo = null;
                    if (!string.IsNullOrEmpty(syndrome.DiagnosisInfo))
                    {
                        try
                        {
                            diagnosisInfo = JsonSerializer.Deserialize<DiagnosisInfo>(syndrome.DiagnosisInfo);
                        }
                        catch (JsonException)
                        {
                            diagnosisInfo = new DiagnosisInfo();
                        }
                    }
                    else
                    {
                        diagnosisInfo = new DiagnosisInfo();
                    }

                    // 反序列化治疗护理信息
                    TreatmentCareInfo? treatmentCareInfo = null;
                    if (!string.IsNullOrEmpty(syndrome.TreatmentCareInfo))
                    {
                        try
                        {
                            treatmentCareInfo = JsonSerializer.Deserialize<TreatmentCareInfo>(syndrome.TreatmentCareInfo);
                        }
                        catch (JsonException)
                        {
                            treatmentCareInfo = new TreatmentCareInfo();
                        }
                    }
                    else
                    {
                        treatmentCareInfo = new TreatmentCareInfo();
                    }

                    var data = new
                    {
                        syndromeName = syndrome.SyndromeName,
                        confidence = syndrome.Confidence,
                        description = syndrome.Description,
                        pathogenesisAnalysis = syndrome.PathogenesisAnalysis,
                        treatmentPrinciple = syndrome.TreatmentPrinciple,
                        recommendedFormulas = !string.IsNullOrEmpty(syndrome.RecommendedFormulas) 
                            ? JsonSerializer.Deserialize<List<string>>(syndrome.RecommendedFormulas) 
                            : new List<string>(),
                        mainSymptoms = !string.IsNullOrEmpty(syndrome.MainSymptoms) 
                            ? JsonSerializer.Deserialize<List<string>>(syndrome.MainSymptoms) 
                            : new List<string>(),
                        commonDiseases = !string.IsNullOrEmpty(syndrome.CommonDiseases) 
                            ? JsonSerializer.Deserialize<List<string>>(syndrome.CommonDiseases) 
                            : new List<string>(),
                        // 新增字段的序列化
                        syndromeCategories = !string.IsNullOrEmpty(syndrome.SyndromeCategories) 
                            ? JsonSerializer.Deserialize<List<string>>(syndrome.SyndromeCategories) 
                            : new List<string>(),
                        relatedOrgans = !string.IsNullOrEmpty(syndrome.RelatedOrgans) 
                            ? JsonSerializer.Deserialize<List<string>>(syndrome.RelatedOrgans) 
                            : new List<string>(),
                        diagnosisInfo = diagnosisInfo,
                        treatmentCareInfo = treatmentCareInfo
                    };
                    return new JsonResult(new { success = true, data });
                case 3: // 获取详情失败
                    return new JsonResult(new { success = false, status = "error", message = "获取详情失败，请重试" });
                default:
                    return new JsonResult(new { success = false, message = "未知状态" });
            }
        }

        /// <summary>
        /// 确认证候并跳转到治疗方案页面
        /// </summary>
        /// <param name="syndromeId">证候ID</param>
        /// <returns>重定向到治疗方案页面</returns>
        public async Task<IActionResult> OnPostConfirmSyndromeAsync(int syndromeId)
        {
            try
            {
                await _syndromeDomain.ConfirmSyndromeAsync(syndromeId);
                return RedirectToPage("Treatment", new { syndromeId = syndromeId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"确认证候失败: {ex.Message}";
                return RedirectToPage(new { visitId = VisitId });
            }
        }

        /// <summary>
        /// 获取证候列表（AJAX调用）
        /// </summary>
        /// <param name="visitId">就诊ID</param>
        /// <returns>JSON格式的证候列表</returns>
        public async Task<IActionResult> OnGetSyndromeListAsync(int visitId)
        {
            var syndromes = await _syndromeDomain.GetSyndromesByVisitIdAsync(visitId);
            
            var result = syndromes.Select(s => new
            {
                syndromeId = s.SyndromeId,
                syndromeName = s.SyndromeName,
                confidence = s.Confidence,
                mainSymptoms = string.IsNullOrEmpty(s.MainSymptoms) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(s.MainSymptoms),
                commonDiseases = string.IsNullOrEmpty(s.CommonDiseases) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(s.CommonDiseases),
                detailStatus = s.DetailStatus,
                isConfirmed = s.IsConfirmed
            }).ToList();

            return new JsonResult(result);
        }


    }
}
