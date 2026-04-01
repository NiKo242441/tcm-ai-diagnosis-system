using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Entities.Dtos;

namespace TcmAiDiagnosis.Web.Areas.Doctor.Pages.Patient
{
    [Authorize(Roles = "Doctor")]
    public class IndexModel : PageModel
    {
        private readonly PatientDomain _patientDomain;
        private readonly UserDomain _userDomain;

        public IndexModel(PatientDomain patientDomain, UserDomain userDomain)
        {
            _patientDomain = patientDomain;
            _userDomain = userDomain;
        }

        public List<User> Patients { get; set; } = new List<User>();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public string SearchKeyword { get; set; } = "";
        public bool IsTenant { get; set; } = false;

        [BindProperty]
        public string CrossTenantSearchPhone { get; set; } = "";

        public List<PatientSearchResultDto> CrossTenantSearchResults { get; set; } = new List<PatientSearchResultDto>();

        public async Task<IActionResult> OnGetAsync(bool isTenant = false, int page = 1, string search = "")
        {
            var user = await _userDomain.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            CurrentPage = page;
            SearchKeyword = search;
            IsTenant = isTenant;

            if (isTenant)
            {
                var request = new PagedRequest { Page = page, PageSize = PageSize, SearchKeyword = search };
                var result = await _patientDomain.GetPatientsByTenantAsync(request, user.TenantId ?? 0);
                Patients = result.Items;
                TotalCount = result.TotalCount;
            }
            else
            {
                var request = new PagedRequest { Page = page, PageSize = PageSize, SearchKeyword = search };
                var result = await _patientDomain.GetPatientsByDoctorAsync(request, user.Id);
                Patients = result.Items;
                TotalCount = result.TotalCount;
            }

            TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);

            return Page();
        }

        /// <summary>
        /// 根据手机号搜索患者
        /// </summary>
        public async Task<IActionResult> OnPostSearchPatientByPhoneAsync([FromBody] SearchPatientRequest request)
        {
            try
            {
                var user = await _userDomain.GetUserAsync(User);
                if (user == null)
                {
                    return new JsonResult(new { success = false, message = "用户未登录" });
                }

                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return new JsonResult(new { success = false, message = "请输入手机号" });
                }

                var patients = await _patientDomain.SearchPatientsByPhoneAsync(request.PhoneNumber, user.TenantId ?? 0);
                
                var results = new List<PatientSearchResultDto>();
                foreach (var patient in patients)
                {
                    var isLinked = await _patientDomain.IsPatientLinkedToTenantAsync(patient.Id, user.TenantId ?? 0);
                    var linkedTenantCount = await _patientDomain.GetPatientLinkedTenantCountAsync(patient.Id);
                    var originalTenant = await _patientDomain.GetPatientOriginalTenantAsync(patient.Id);

                    results.Add(new PatientSearchResultDto
                    {
                        PatientId = patient.Id,
                        FullName = patient.FullName,
                        PhoneNumber = patient.PhoneNumber,
                        Gender = patient.Gender,
                        DateOfBirth = patient.DateOfBirth,
                        IdCard = null, // TODO: 需要在 UserDetail 中添加 IdCard 属性
                        IsLinkedToCurrentTenant = isLinked,
                        LinkedTenantCount = linkedTenantCount,
                        OriginalTenantName = originalTenant?.TenantName ?? ""
                    });
                }

                return new JsonResult(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 跨租户搜索患者
        /// </summary>
        public async Task<IActionResult> OnPostSearchCrossTenantPatientAsync()
        {
            try
            {
                var user = await _userDomain.GetUserAsync(User);
                if (user == null)
                {
                    return new JsonResult(new { success = false, message = "用户未登录" });
                }

                if (string.IsNullOrWhiteSpace(CrossTenantSearchPhone))
                {
                    return new JsonResult(new { success = false, message = "请输入手机号" });
                }

                var patients = await _patientDomain.SearchPatientsByPhoneAsync(CrossTenantSearchPhone, user.TenantId ?? 0);
                
                var results = new List<PatientSearchResultDto>();
                foreach (var patient in patients)
                {
                    var isLinked = await _patientDomain.IsPatientLinkedToTenantAsync(patient.Id, user.TenantId ?? 0);
                    var linkedTenantCount = await _patientDomain.GetPatientLinkedTenantCountAsync(patient.Id);
                    var originalTenant = await _patientDomain.GetPatientOriginalTenantAsync(patient.Id);

                    results.Add(new PatientSearchResultDto
                    {
                        PatientId = patient.Id,
                        FullName = patient.FullName,
                        PhoneNumber = patient.PhoneNumber,
                        Gender = patient.Gender,
                        DateOfBirth = patient.DateOfBirth,
                        IdCard = null, // TODO: 需要在 UserDetail 中添加 IdCard 属性
                        IsLinkedToCurrentTenant = isLinked,
                        LinkedTenantCount = linkedTenantCount,
                        OriginalTenantName = originalTenant?.TenantName ?? ""
                    });
                }

                return new JsonResult(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 添加现有患者到当前租户
        /// </summary>
        public async Task<IActionResult> OnPostAddExistingPatientAsync([FromBody] AddExistingPatientRequest request)
        {
            try
            {
                var user = await _userDomain.GetUserAsync(User);
                if (user == null)
                {
                    return new JsonResult(new { success = false, message = "用户未登录" });
                }

                var success = await _patientDomain.AddExistingPatientToTenantAsync(
                    request.PatientId, 
                    user.Id, 
                    user.TenantId ?? 0);

                if (success)
                {
                    return new JsonResult(new { success = true, message = "患者添加成功" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "患者添加失败" });
                }
            }
            catch (InvalidOperationException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "系统错误：" + ex.Message });
            }
        }

        /// <summary>
        /// 身份证脱敏处理
        /// </summary>
        private string? MaskIdCard(string? idCard)
        {
            if (string.IsNullOrEmpty(idCard) || idCard.Length < 8)
                return idCard;

            return idCard.Substring(0, 4) + "****" + idCard.Substring(idCard.Length - 4);
        }
    }

    /// <summary>
    /// 搜索患者请求模型
    /// </summary>
    public class SearchPatientRequest
    {
        public string PhoneNumber { get; set; } = "";
    }
}