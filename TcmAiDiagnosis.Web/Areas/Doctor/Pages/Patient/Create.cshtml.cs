using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Areas.Doctor.Pages.Patient
{
    public class CreateModel : PageModel
    {
        private readonly PatientDomain _patientDomain;
        private readonly UserDomain _userDomain;

        public CreateModel(PatientDomain patientDomain, UserDomain userDomain)
        {
            _patientDomain = patientDomain;
            _userDomain = userDomain;
        }

        [BindProperty]
        public User Patient { get; set; } = new() { Detail = new() };

        public int CurrentDoctorId { get; set; }
        public int CurrentTenantId { get; set; }

        public List<MedicalHistory> ChronicDiseases { get; set; } = new();
        public List<MedicalHistory> InfectiousDiseases { get; set; } = new();
        public List<MedicalHistory> AllergyHistories { get; set; } = new();
        public List<MedicalHistory> FamilyHistories { get; set; } = new();

        public async Task OnGetAsync(string phoneNumber = null)
        {
            await LoadDataAsync();
            
            // 如果提供了手机号参数，预填到表单中
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                Patient.PhoneNumber = phoneNumber;
            }
        }

        public async Task<IActionResult> OnPostCreatePatientAsync()
        {
            try
            {
                var user = await _userDomain.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }
                
                var userPassword = new string((Patient?.PhoneNumber ?? "").Skip(3).ToArray());
                
                // 确保Patient.Detail不为null
                if (Patient.Detail == null)
                {
                    Patient.Detail = new UserDetail();
                }
                
                Patient.Detail.PastMedicalHistory = string.Join(",", SelectedChronicDiseases);
                Patient.Detail.InfectiousDiseaseHistory = string.Join(",", SelectedInfectiousDiseases);
                Patient.Detail.AllergyHistory = string.Join(",", SelectedAllergyHistories);
                Patient.Detail.FamilyMedicalHistory = string.Join(",", SelectedFamilyHistories);

                //await _patientDomain.AddPatientAsync(Patient, Patient.Detail, userPassword, user.Id, user.TenantId ?? 0);
                if (user.TenantId == null)
                {
                    ModelState.AddModelError("", "当前医生未绑定医疗机构，无法创建患者");
                    await LoadDataAsync();
                    return Page();
                }

                await _patientDomain.AddPatientAsync(
                    Patient,
                    Patient.Detail,
                    userPassword,
                    user.Id,
                    user.TenantId.Value
                );
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.Clear();
                ModelState.AddModelError(string.Empty, "创建患者失败：" + ex.Message);
                await LoadDataAsync();
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
            var user = await _userDomain.GetUserAsync(User);
            if (user != null)
            {
                CurrentDoctorId = user.Id;
                CurrentTenantId = user.TenantId ?? 0;
            }

            ChronicDiseases = await _patientDomain.GetMedicalHistoriesByTypeAsync("慢性疾病");
            InfectiousDiseases = await _patientDomain.GetMedicalHistoriesByTypeAsync("传染病史");
            AllergyHistories = await _patientDomain.GetMedicalHistoriesByTypeAsync("过敏史");
            FamilyHistories = await _patientDomain.GetMedicalHistoriesByTypeAsync("家族病史");
        }
    }
}