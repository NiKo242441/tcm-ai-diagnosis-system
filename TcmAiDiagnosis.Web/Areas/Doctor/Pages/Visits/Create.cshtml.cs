using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Areas.Doctor.Pages.Visits
{
    public class CreateModel : PageModel
    {
        private readonly PatientDomain _patientDomain;
        private readonly VisitDomain _visitDomain;
        private readonly UserDomain _userDomain;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(PatientDomain patientDomain, VisitDomain visitDomain, UserDomain userDomain,
                          IHttpContextAccessor httpContextAccessor, ILogger<CreateModel> logger)
        {
            _patientDomain = patientDomain;
            _visitDomain = visitDomain;
            _userDomain = userDomain;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int PatientId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SeriesId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? VisitTypeParam { get; set; }

        [BindProperty]
        public User Patient { get; set; }

        [BindProperty]
        public Visit NewVisit { get; set; } = new Visit() { };

        [BindProperty]
        public List<string> TongueSymptoms { get; set; } = new List<string>();

        [BindProperty]
        public List<string> MentalState { get; set; } = new List<string>();

        [BindProperty]
        public List<string> PulseFeatures { get; set; } = new List<string>();

        [BindProperty]
        public List<string> Sweating { get; set; } = new List<string>();

        [BindProperty]
        public List<string> Diet { get; set; } = new List<string>();

        [BindProperty]
        public List<string> Excretion { get; set; } = new List<string>();

        [BindProperty]
        public List<string> Sleep { get; set; } = new List<string>();

        [BindProperty]
        public List<string> EmotionState { get; set; } = new List<string>();

        [BindProperty]
        public List<string> TongueShape { get; set; } = new List<string>();

        [BindProperty]
        public List<string> Constitution { get; set; } = new List<string>();

        public Entities.VisitSeries? ActiveSeries { get; set; }

        public int CurrentDoctorId { get; set; }

        public int CurrentTenantId { get; set; }

        public List<MedicalHistory> ChronicDiseases { get; set; } = new List<MedicalHistory>();
        public List<MedicalHistory> InfectiousDiseases { get; set; } = new List<MedicalHistory>();
        public List<MedicalHistory> AllergyHistories { get; set; } = new List<MedicalHistory>();
        public List<MedicalHistory> FamilyHistories { get; set; } = new List<MedicalHistory>();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
            await LoadUserAsync();
            await _visitDomain.UpdateVisitNotes();
            Patient = await _patientDomain.GetPatientByIdAsync(PatientId);
            if (Patient == null)
            {
                _logger.LogWarning("患者ID {PatientId} 不存在", PatientId);
                throw new Exception("未找到指定患者");
            }

            // 处理就诊系列逻辑
            if (SeriesId.HasValue && SeriesId > 0)
            {
                // 指定了系列ID，进行复诊
                ActiveSeries = await _visitDomain.GetVisitSeriesByIdAsync(SeriesId.Value);
                if (ActiveSeries == null)
                {
                    _logger.LogWarning("就诊系列ID {SeriesId} 不存在", SeriesId);
                    throw new Exception("未找到指定的就诊系列");
                }
                if (ActiveSeries.PatientUserId != Patient.Id)
                {
                    _logger.LogWarning("就诊系列 {SeriesId} 不属于患者 {PatientId}", SeriesId, PatientId);
                    throw new Exception("就诊系列与患者不匹配");
                }
                _logger.LogInformation("进行复诊，患者ID: {PatientId}，系列ID: {SeriesId}", PatientId, SeriesId);
            }
            else
            {
                // 没有指定系列ID，开始新的就诊系列（初诊）
                ActiveSeries = null;
                _logger.LogInformation("开始新的就诊系列（初诊），患者ID: {PatientId}", PatientId);
            }
        }

        private async Task LoadDataAsync()
        {
            // 获取各类病史基础数据
            ChronicDiseases = await _patientDomain.GetMedicalHistoriesByTypeAsync("慢性疾病");
            InfectiousDiseases = await _patientDomain.GetMedicalHistoriesByTypeAsync("传染病史");
            AllergyHistories = await _patientDomain.GetMedicalHistoriesByTypeAsync("过敏史");
            FamilyHistories = await _patientDomain.GetMedicalHistoriesByTypeAsync("家族病史");
        }
        private async Task LoadUserAsync()
        {
            var user = await _userDomain.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError("当前用户未登录");
                throw new Exception("用户未登录");
            }
            CurrentDoctorId = user.Id;
            CurrentTenantId = user.TenantId ?? throw new Exception("用户未分配租户");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadUserAsync();
            // 重新加载患者信息，确保Patient不为null
            Patient = await _patientDomain.GetPatientByIdAsync(PatientId);
            if (Patient == null)
            {
                _logger.LogWarning("患者ID {PatientId} 不存在", PatientId);
                ModelState.AddModelError(string.Empty, "未找到指定患者");
                await LoadDataAsync();
                return Page();
            }

            // 获取统一时间戳
            var currentTime = DateTime.Now;

            // 处理多选框数据
            NewVisit.TongueSymptoms = string.Join("、", TongueSymptoms);
            NewVisit.MentalState = string.Join("、", MentalState);
            NewVisit.PulseFeatures = string.Join("、", PulseFeatures);
            NewVisit.Sweating = string.Join("、", Sweating);
            NewVisit.Diet = string.Join("、", Diet);
            NewVisit.Excretion = string.Join("、", Excretion);
            NewVisit.Sleep = string.Join("、", Sleep);
            NewVisit.EmotionState = string.Join("、", EmotionState);
            NewVisit.TongueShape = string.Join("、", TongueShape);
            NewVisit.ConstitutionAssessment = string.Join("、", Constitution);

            // 设置就诊基本信息
            NewVisit.PatientUserId = Patient.Id;
            NewVisit.VisitDate = currentTime;
            NewVisit.CreatedAt = currentTime;
            NewVisit.UpdatedAt = currentTime;
            NewVisit.DoctorUserId = CurrentDoctorId;
            NewVisit.TenantId = CurrentTenantId;

            // 根据参数设置就诊类型
            if (!SeriesId.HasValue || SeriesId == 0)
            {
                // 新系列，设置为初诊
                NewVisit.VisitType = VisitType.Initial;
            }
            else
            {
                // 现有系列，根据VisitTypeParam参数决定是复诊还是随访
                if (VisitTypeParam == "followupcall")
                {
                    NewVisit.VisitType = VisitType.FollowUpCall;
                }
                else
                {
                    NewVisit.VisitType = VisitType.FollowUp;
                }
            }

            try
            {
                // 使用事务确保数据一致性
                // 处理就诊系列
                if (!SeriesId.HasValue || SeriesId == 0)
                {
                    // 没有指定系列ID，创建新的就诊系列（初诊）
                    _logger.LogInformation("开始创建新就诊系列流程（初诊），患者ID: {PatientId}", Patient.Id);
                    
                    // 先结束患者所有未结束的就诊系列
                    await _visitDomain.UpdatePatientActiveSeriesAsync(Patient.Id, CurrentTenantId, currentTime);
                    
                    // 创建新就诊系列和就诊记录
                    await _visitDomain.AddVisitAsync(NewVisit, CurrentDoctorId, CurrentTenantId);
                    
                    _logger.LogInformation("成功创建初诊系列，患者ID: {PatientId}", Patient.Id);
                }
                else
                {
                    // 指定了系列ID，在现有系列中添加就诊记录（复诊）
                    _logger.LogInformation("开始复诊流程，患者ID: {PatientId}, 系列ID: {SeriesId}", Patient.Id, SeriesId);
                    
                    // 设置复诊序列
                    NewVisit.SeriesId = SeriesId.Value;
                    
                    // 添加复诊记录（内部更新就诊系列）
                    await _visitDomain.AddVisitAsync(NewVisit, CurrentDoctorId, CurrentTenantId);
                    
                    _logger.LogInformation("成功添加复诊记录，患者ID: {PatientId}, 系列ID: {SeriesId}", Patient.Id, SeriesId);
                }

                _logger.LogInformation("就诊记录保存成功，就诊ID: {VisitId}", NewVisit.VisitId);

                return RedirectToPage("Syndrome", new { visitId = NewVisit.VisitId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存就诊记录失败：{ex.Message}");
                ModelState.AddModelError(string.Empty, "保存就诊记录失败: " + ex.Message);
                await LoadDataAsync();
                return Page();
            }
        }
    }
}
