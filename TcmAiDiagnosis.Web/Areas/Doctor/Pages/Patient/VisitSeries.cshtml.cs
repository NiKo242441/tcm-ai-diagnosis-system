using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Areas.Doctor.Pages.Patient
{
    /// <summary>
    /// 患者就诊系列列表页面模型
    /// </summary>
    public class VisitSeriesModel : PageModel
    {
        private readonly PatientDomain _patientDomain;
        private readonly VisitDomain _visitDomain;
        private readonly UserDomain _userDomain;

        public VisitSeriesModel(PatientDomain patientDomain, VisitDomain visitDomain, UserDomain userDomain)
        {
            _patientDomain = patientDomain;
            _visitDomain = visitDomain;
            _userDomain = userDomain;
        }

        /// <summary>
        /// 患者信息
        /// </summary>
        public User Patient { get; set; } = new();

        /// <summary>
        /// 当前医生ID
        /// </summary>
        public int CurrentDoctorId { get; set; }

        /// <summary>
        /// 当前租户ID
        /// </summary>
        public int CurrentTenantId { get; set; }

        /// <summary>
        /// 患者当前进行中的就诊系列
        /// </summary>
        public Entities.VisitSeries? ActiveSeries { get; set; }

        /// <summary>
        /// 患者所有就诊系列（按时间倒序）
        /// </summary>
        public List<Entities.VisitSeries> AllSeries { get; set; } = new();

        /// <summary>
        /// 页面GET请求处理
        /// </summary>
        /// <param name="patientId">患者ID</param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(int patientId)
        {
            try
            {
                // 加载当前用户信息
                await LoadCurrentUserAsync();

                // 加载患者信息
                Patient = await _patientDomain.GetPatientByIdAsync(patientId);
                if (Patient == null)
                {
                    return NotFound("未找到指定患者");
                }

                // 加载患者就诊系列信息
                await LoadVisitSeriesAsync(patientId);

                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                // 记录异常日志（实际项目中应添加日志记录）
                ModelState.AddModelError(string.Empty, "加载页面时发生错误：" + ex.Message);
                return Page();
            }
        }

        /// <summary>
        /// 加载当前登录用户信息
        /// </summary>
        /// <returns></returns>
        private async Task LoadCurrentUserAsync()
        {
            var user = await _userDomain.GetUserAsync(User);
            if (user == null)
            {
                throw new UnauthorizedAccessException("用户未登录或登录已过期");
            }
            CurrentDoctorId = user.Id;
            CurrentTenantId = user.TenantId ?? 0;
        }

        /// <summary>
        /// 加载患者就诊系列数据
        /// </summary>
        /// <param name="patientId">患者ID</param>
        /// <returns></returns>
        private async Task LoadVisitSeriesAsync(int patientId)
        {
            // 查询当前进行中的就诊系列
            ActiveSeries = await _visitDomain.QueryVisitSeriesByPatientAsync(patientId, CurrentTenantId, 0);

            // 查询所有就诊系列（进行中和已完成的）
            var allSeriesResult = await _visitDomain.QueryAllVisitSeriesByPatientAsync(patientId, CurrentTenantId);
            AllSeries = allSeriesResult.ToList();
        }
    }
}