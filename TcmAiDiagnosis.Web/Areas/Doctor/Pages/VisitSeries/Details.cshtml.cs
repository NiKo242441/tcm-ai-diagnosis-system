using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Areas.Doctor.Pages.VisitSeries
{
    /// <summary>
    /// 就诊系列详情页面模型
    /// </summary>
    [Authorize]
    [Area("Doctor")]
    public class DetailsModel : PageModel
    {
        private readonly VisitDomain _visitDomain;
        private readonly PatientDomain _patientDomain;
        private readonly UserManager<User> _userManager;

        public DetailsModel(VisitDomain visitDomain, PatientDomain patientDomain, UserManager<User> userManager)
        {
            _visitDomain = visitDomain;
            _patientDomain = patientDomain;
            _userManager = userManager;
        }

        /// <summary>
        /// 就诊系列信息
        /// </summary>
        public Entities.VisitSeries VisitSeries { get; set; } = new();

        /// <summary>
        /// 患者信息
        /// </summary>
        public User Patient { get; set; } = new();

        /// <summary>
        /// 系列内的所有就诊记录
        /// </summary>
        public List<Visit> Visits { get; set; } = new();

        /// <summary>
        /// 当前用户
        /// </summary>
        public User CurrentUser { get; set; } = new();

        /// <summary>
        /// 页面加载
        /// </summary>
        /// <param name="id">就诊系列ID</param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // 加载当前用户
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }
                CurrentUser = currentUser;

                // 获取就诊系列信息
                var visitSeries = await _visitDomain.GetVisitSeriesByIdAsync(id);
                if (visitSeries == null)
                {
                    TempData["ErrorMessage"] = "未找到指定的就诊系列";
                    return RedirectToPage("/Patient/Index");
                }
                VisitSeries = visitSeries;

                // 获取患者信息
                var patient = await _patientDomain.GetPatientByIdAsync(VisitSeries.PatientUserId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "未找到相关患者信息";
                    return RedirectToPage("/Patient/Index");
                }
                Patient = patient;

                // 获取系列内的所有就诊记录，按时间倒序排列
                Visits = (await _visitDomain.GetVisitsBySeriesIdAsync(id))
                    .OrderByDescending(v => v.VisitDate)
                    .ToList();

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"加载就诊系列详情时发生错误：{ex.Message}";
                return RedirectToPage("/Patient/Index");
            }
        }

        /// <summary>
        /// 结束就诊系列
        /// </summary>
        /// <param name="id">就诊系列ID</param>
        /// <param name="endReason">结束原因</param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostEndSeriesAsync(int id, string endReason)
        {
            try
            {
                // 加载当前用户
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }
                CurrentUser = currentUser;

                // 结束就诊系列
                await _visitDomain.EndSeriesAsync(id, endReason, CurrentUser.Id);

                TempData["SuccessMessage"] = "就诊系列已成功结束";
                return RedirectToPage("Details", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"结束就诊系列时发生错误：{ex.Message}";
                return RedirectToPage("Details", new { id = id });
            }
        }
    }
}