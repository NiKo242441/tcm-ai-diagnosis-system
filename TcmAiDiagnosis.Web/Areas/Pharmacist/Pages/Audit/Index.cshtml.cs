using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Entities.Enums;
using TcmAiDiagnosis.Web.Areas.Doctor.Pages.Patient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TcmAiDiagnosis.Web.Areas.Pharmacist.Pages.Audit
{
    [Authorize(Roles = "Pharmacist")]
    public class IndexModel : PageModel
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly PatientDomain _patientDomain;
        private readonly UserDomain _userDomain;

        public IndexModel(TcmAiDiagnosisContext context, PatientDomain patientDomain, UserDomain userDomain)
        {
            _context = context;
            _patientDomain = patientDomain;
            _userDomain = userDomain;
        }

        public List<PlanWithPatientDto> PendingPlans { get; set; }
        public List<User> Patients { get; set; } = new List<User>();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public string SearchKeyword { get; set; } = "";
        public bool IsTenant { get; set; } = false;
        public DateTime? VisitDate { get; set; }
        [BindProperty]
        public string CrossTenantSearchPhone { get; set; } = "";
        [BindProperty]
        public bool NoResults { get; set; } = false;

        public List<PatientSearchResultDto> CrossTenantSearchResults { get; set; } = new List<PatientSearchResultDto>();

        public async Task<IActionResult> OnGetAsync(bool isTenant = false, int page = 1, string search = "")
        {
            var query = _context.Treatments
        .Include(t => t.Patient)
        .Where(t => t.Status == TreatmentStatus.Checking);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Patient.PhoneNumber.Contains(search) ||
                                         t.Patient.FullName.Contains(search));
            }

            var user = await _userDomain.GetUserAsync(User);
            //if (user == null)
            //{
            //    return RedirectToPage("/Account/Login", new { area = "Identity" });
            //}

            PendingPlans = await _context.Treatments
                .Where(t => t.Status == TreatmentStatus.Checking)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new PlanWithPatientDto
                {
                    Id = t.Id,
                    PatientId = t.PatientId,
                    CreatedAt = t.CreatedAt,

                    // 关联 Patient
                    PatientName = t.Patient.FullName,
                    PhoneNumber = t.Patient.PhoneNumber,
                    Gender = t.Patient.Gender,
                    DateOfBirth = t.Patient.DateOfBirth,
                })
                .ToListAsync();

            NoResults = !string.IsNullOrWhiteSpace(SearchKeyword) && !PendingPlans.Any();

            CurrentPage = page;
            SearchKeyword = search;
            IsTenant = isTenant;
            SearchKeyword = search;

            // 总数和总页数需要更新
            TotalCount = PendingPlans.Count;
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
    }
}