using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Areas.Doctor.Pages.Patient
{
    public class DetailModel : PageModel
    {
        private readonly PatientDomain _patientDomain;
        private readonly VisitDomain _visitDomain;
        private readonly UserDomain _userDomain;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DetailModel(PatientDomain patientDomain, VisitDomain visitDomain, UserDomain userDomain, IHttpContextAccessor httpContextAccessor)
        {
            _patientDomain = patientDomain;
            _visitDomain = visitDomain;
            _userDomain = userDomain;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]
        public User Patient { get; set; } = new();

        public int CurrentDoctorId { get; set; }

        public int CurrentTenantId { get; set; }



        public List<MedicalHistory> ChronicDiseases { get; set; } = new List<MedicalHistory>();
        public List<MedicalHistory> InfectiousDiseases { get; set; } = new List<MedicalHistory>();
        public List<MedicalHistory> AllergyHistories { get; set; } = new List<MedicalHistory>();
        public List<MedicalHistory> FamilyHistories { get; set; } = new List<MedicalHistory>();

        public async Task OnGetAsync(int patientId)
        {
            await LoadDataAsync();
            await LoadCurrentUser();
            
            Patient = await _patientDomain.GetPatientByIdAsync(patientId);
            if (Patient == null)
            {
                throw new ArgumentException("未找到指定患者");
            }

            // 确保患者详情信息存在，如果不存在则创建一个空的详情对象
            if (Patient.Detail == null)
            {
                Patient.Detail = new UserDetail
                {
                    UserId = Patient.Id,
                    PastMedicalHistory = string.Empty,
                    InfectiousDiseaseHistory = string.Empty,
                    AllergyHistory = string.Empty,
                    FamilyMedicalHistory = string.Empty,
                    EmergencyContactName = string.Empty,
                    EmergencyContactPhone = string.Empty,
                    HomeAddress = string.Empty
                };
            }

            // 加载患者病史信息
            if (!string.IsNullOrWhiteSpace(Patient.Detail.PastMedicalHistory)) 
                SelectedChronicDiseases = Patient.Detail.PastMedicalHistory.Split(",").ToList();
            if (!string.IsNullOrWhiteSpace(Patient.Detail.InfectiousDiseaseHistory)) 
                SelectedInfectiousDiseases = Patient.Detail.InfectiousDiseaseHistory.Split(",").ToList();
            if (!string.IsNullOrWhiteSpace(Patient.Detail.AllergyHistory)) 
                SelectedAllergyHistories = Patient.Detail.AllergyHistory.Split(",").ToList();
            if (!string.IsNullOrWhiteSpace(Patient.Detail.FamilyMedicalHistory)) 
                SelectedFamilyHistories = Patient.Detail.FamilyMedicalHistory.Split(",").ToList();
        }

        /// <summary>
        /// 处理更新患者信息的POST请求
        /// </summary>
        /// <returns>重定向或当前页面</returns>
        public async Task<IActionResult> OnPostUpdatePatientAsync()
        {
            try
            {
                await LoadCurrentUser();
                
                // 确保患者详情信息存在，如果不存在则创建一个空的详情对象
                if (Patient.Detail == null)
                {
                    Patient.Detail = new UserDetail
                    {
                        UserId = Patient.Id,
                        PastMedicalHistory = string.Empty,
                        InfectiousDiseaseHistory = string.Empty,
                        AllergyHistory = string.Empty,
                        FamilyMedicalHistory = string.Empty,
                        EmergencyContactName = string.Empty,
                        EmergencyContactPhone = string.Empty,
                        HomeAddress = string.Empty
                    };
                }
                
                // 调用领域服务更新患者信息
                Patient.Detail.PastMedicalHistory = string.Join(",", SelectedChronicDiseases);
                Patient.Detail.InfectiousDiseaseHistory = string.Join(",", SelectedInfectiousDiseases);
                Patient.Detail.AllergyHistory = string.Join(",", SelectedAllergyHistories);
                Patient.Detail.FamilyMedicalHistory = string.Join(",", SelectedFamilyHistories);
                await _patientDomain.UpdatePatientAsync(Patient, Patient.Detail, CurrentDoctorId, CurrentTenantId);
                // 更新成功后重定向到当前患者详情页
                return RedirectToPage("Detail", new { patientId = Patient.Id });
            }
            catch (Exception ex)
            {
                // 记录异常日志（实际项目中应添加日志记录）
                ModelState.Clear();
                ModelState.AddModelError(string.Empty, "更新患者信息时发生错误：" + ex.Message);
                await LoadDataAsync(); // 重新加载基础数据
                return Page();
            }
        }

        [BindProperty]
        public List<string> SelectedChronicDiseases { get; set; } = new();

        [BindProperty]
        public List<string> SelectedInfectiousDiseases { get; set; } = new();

        [BindProperty]
        public List<string> SelectedAllergyHistories { get; set; } = new();

        [BindProperty]
        public List<string> SelectedFamilyHistories { get; set; } = new();

        private async Task LoadDataAsync()
        {
            ChronicDiseases = await _patientDomain.GetMedicalHistoriesByTypeAsync("慢性疾病");
            InfectiousDiseases = await _patientDomain.GetMedicalHistoriesByTypeAsync("传染病史");
            AllergyHistories = await _patientDomain.GetMedicalHistoriesByTypeAsync("过敏史");
            FamilyHistories = await _patientDomain.GetMedicalHistoriesByTypeAsync("家族病史");
        }

        private async Task LoadCurrentUser()
        {
            var user = await _userDomain.GetUserAsync(User);
            if (user == null)
            {
                throw new UnauthorizedAccessException("用户未登录或登录已过期");
            }
            CurrentDoctorId = user.Id;
            CurrentTenantId = user.TenantId ?? 0;
        }


    }
}
