using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.EFContext.Mapper;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Entities.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TcmAiDiagnosis.Web.Areas.Pharmacist.Pages.Audit
{
    public class DetailModel : PageModel
    {
        private readonly TcmAiDiagnosisContext _context;

        public DetailModel(TcmAiDiagnosisContext context)
        {
            _context = context;
        }

        public Treatment TreatmentPlan { get; set; }
        public SyndromeDetailDto? SyndromeDetail { get; set; }

        [BindProperty]
        public int SyndromeId { get; set; }

        [BindProperty]
        public string PrimarySyndrome { get; set; }

        [BindProperty]
        public string AccompanyingSyndromes { get; set; }

        [BindProperty]
        public string ConstitutionType { get; set; }
        [BindProperty]
        public List<string> SelectedFormulas { get; set; }  // 方剂复选框
        [BindProperty]
        public string RecommendedFormulas { get; set; }     // 推荐方剂
        [BindProperty]
        public string SyndromeCategories { get; set; }      // 证候分类
        [BindProperty]
        public string RelatedOrgans { get; set; }      // 归属脏腑
        [BindProperty]
        public string CommonDiseases { get; set; }      // 常见疾病
        [BindProperty]
        public string MainSymptoms { get; set; }      // 主要疾病
        [BindProperty]
        public string PathogenesisAnalysis { get; set; }      // 病机概要
        [BindProperty]
        public string TCMConclusion { get; set; }       // 中医辨证
        [BindProperty]
        public string AccompanyingBasis { get; set; }       // 临床关联
        [BindProperty]
        public string TreatmentPrinciple { get; set; }      // 治疗原理
        public List<Herb> FormulaDetails { get; set; } = new();     // 中药处方列表，从json中解析出来

        // 直接暴露给前端的 DTO
        public TreatmentDto Treatment { get; set; } = new TreatmentDto();
        public DietaryWarningDto DietaryWarning { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var treatment = await _context.Treatments
                .Include(t => t.Patient)
                .Include(t => t.Syndrome)
                .Include(t => t.Prescriptions)
                    .ThenInclude(p => p.PrescriptionItems)
                .Include(t => t.Acupunctures)
                .Include(t => t.Moxibustions)
                .Include(t => t.Cuppings)
                .Include(t => t.DietaryTherapies)
                    .ThenInclude(d => d.DietaryTherapyIngredients)
                .Include(t => t.LifestyleAdvices)
                .Include(t => t.DietaryAdvices)
                .Include(t => t.FollowUpAdvices)
                .Include(t => t.HerbalWarnings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (treatment == null)
                return NotFound();

            var entity = treatment.Syndrome;

            if (entity == null)
            {
                TempData["Message"] = "未找到对应的证候数据。";
                return RedirectToPage("./Index");
            }

            Treatment = await DetailMapToDtoAsync(treatment);
            TreatmentPlan = treatment;
            SyndromeDetail = MapToDto(entity);

            return Page();
        }

        private SyndromeDetailDto MapToDto(Syndrome s)
        {
            // 防御性检查
            if (s == null)
                return new SyndromeDetailDto();

            // 解析 JSON
            var treatmentCare = DeserializeObject<TreatmentCareInfo>(s.TreatmentCareInfo);

            var dto = new SyndromeDetailDto
            {
                SyndromeName = s.SyndromeName,
                Confidence = s.Confidence,
                Description = s.Description ?? string.Empty,
                PathogenesisAnalysis = s.PathogenesisAnalysis ?? string.Empty,

                MainSymptoms = DeserializeList(s.MainSymptoms),
                CommonDiseases = DeserializeList(s.CommonDiseases),
                RecommendedFormulas = DeserializeList(s.RecommendedFormulas),
                SyndromeCategories = DeserializeList(s.SyndromeCategories),
                RelatedOrgans = DeserializeList(s.RelatedOrgans),

                // 诊断信息
                DiagnosisInfo = DeserializeObject<DiagnosisInfo>(s.DiagnosisInfo),

                // 治疗与护理信息
                TreatmentCareInfo = treatmentCare,

                // 这里把 JSON 里的治疗原则赋值到 DTO 的顶层字段
                TreatmentPrinciple = treatmentCare.TreatmentRecommendation?.TreatmentPrinciple ?? string.Empty,
            };

            // 确保 DiagnosisInfo 不为 null
            dto.DiagnosisInfo ??= new DiagnosisInfo();

            // 确保嵌套对象都不为 null
            dto.DiagnosisInfo.Conclusion ??= new DiagnosisConclusion();
            dto.DiagnosisInfo.Analysis ??= new DiagnosisAnalysis();
            dto.DiagnosisInfo.DifferentialDiagnoses ??= new List<DifferentialDiagnosis>();

            // 确保 AccompanyingSyndromes 不为空
            dto.DiagnosisInfo.Conclusion.AccompanyingSyndromes ??= new List<string>();

            // 治疗护理信息
            dto.TreatmentCareInfo ??= new TreatmentCareInfo();
            dto.TreatmentCareInfo.TreatmentRecommendation ??= new TreatmentRecommendation();
            dto.TreatmentCareInfo.Precautions ??= new Precautions();

            return dto;
        }

        private async Task<TreatmentDto> DetailMapToDtoAsync(Treatment t)
        {
            // 拿到治疗方案用户
            var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Id == t.CreatedByUserId);

            // 计算生日
            var age = t.Patient.DateOfBirth.HasValue
    ? DateTime.Now.Year - t.Patient.DateOfBirth.Value.Year
    : (int?)null;

            return new TreatmentDto
            {
                Id = t.Id,
                PatientId = t.PatientId,
                VisitId = t.VisitId,
                SyndromeId = t.SyndromeId,
                Version = t.Version,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CreatorName = user?.FullName ?? user?.UserName ?? "未知医生",
                IsAiOriginated = t.IsAiOriginated,
                IsLatest = t.IsLatest,

                TcmDiagnosis = t.TcmDiagnosis,
                SyndromeAnalysis = t.SyndromeAnalysis,
                TreatmentPrinciple = t.TreatmentPrinciple,
                ExpectedOutcome = t.ExpectedOutcome,
                Precautions = t.Precautions,

                // 患者信息
                PatientInfo = t.Patient == null ? null : new PatientBriefDto
                {
                    Id = t.Patient.Id,
                    FullName = t.Patient.FullName,
                    Gender = t.Patient.Gender,
                    Age = (int)age
                    //Phone = t.Patient.PhoneNumber
                },

                // 证候信息
                SyndromeInfo = t.Syndrome == null ? null : new SyndromeBriefDto
                {
                    Id = t.Syndrome.SyndromeId,
                    SyndromeName = t.Syndrome.SyndromeName,
                    MainSymptoms = t.Syndrome.MainSymptoms,
                    TreatmentPrinciple = t.TreatmentPrinciple
                },

                // 中药方剂
                Prescriptions = t.Prescriptions.Select(p => new PrescriptionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Efficacy = p.Efficacy,
                    Usage = p.Usage,
                    Description = p.Description,
                    Notes = p.Notes,
                    Items = p.PrescriptionItems.Select(i => new PrescriptionItemDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Dosage = i.Dosage,
                        Unit = i.Unit,
                        ProcessingMethod = i.ProcessingMethod
                    }).ToList()
                }).ToList(),

                // 针灸
                Acupunctures = t.Acupunctures.Select(a => new AcupunctureDto
                {
                    Id = a.Id,
                    PointName = a.PointName,
                    Location = a.Location,
                    Efficacy = a.Efficacy,
                    Duration = a.Duration,
                    Frequency = a.Frequency
                }).ToList(),

                // 艾灸
                Moxibustions = t.Moxibustions.Select(m => new MoxibustionDto
                {
                    Id = m.Id,
                    PointName = m.PointName,
                    Location = m.Location,
                    MoxaType = m.MoxaType,
                    Technique = m.Technique,
                    TemperatureControl = m.TemperatureControl,
                    Method = m.Method,
                    Duration = m.Duration,
                    Frequency = m.Frequency,
                    CourseDuration = m.CourseDuration,
                    Efficacy = m.Efficacy,
                    Indications = m.Indications,
                    TechniquePoints = m.TechniquePoints,
                    Precautions = m.Precautions,
                    Contraindications = m.Contraindications,
                    PostTreatmentCare = m.PostTreatmentCare,
                    CombinationTherapy = m.CombinationTherapy,

                }).ToList(),

                // 拔罐
                Cuppings = t.Cuppings.Select(c => new CuppingDto
                {
                    Id = c.Id,
                    Method = c.Method,
                    Area = c.Area,
                    SpecificPoints = c.SpecificPoints,
                    SuitableFor = c.SuitableFor,
                    CupSize = c.CupSize,
                    CupType = c.CupType,
                    SuctionStrength = c.SuctionStrength,
                    Indications = c.Indications,
                    TechniquePoints = c.TechniquePoints,
                    Efficacy = c.Efficacy,
                    Duration = c.Duration,
                    Precautions = c.Precautions,
                    Frequency = c.Frequency
                }).ToList(),

                // 食疗
                DietaryTherapies = t.DietaryTherapies.Select(d => new DietaryTherapyDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Category = d.Category,
                    Description = d.Description,
                    Preparation = d.Preparation,
                    SuitableFor = d.SuitableFor,
                    Contraindications = d.Contraindications,
                    ServingMethod = d.ServingMethod,
                    StorageMethod = d.StorageMethod,
                    Efficacy = d.Efficacy,
                    PatientFriendlyName = d.PatientFriendlyName,
                    Ingredients = d.DietaryTherapyIngredients.Select(i => new DietaryTherapyIngredientDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Dosage = i.Dosage,
                        ProcessingMethod = i.ProcessingMethod,
                        Notes = i.Notes,
                    }).ToList()
                }).ToList(),

                // 生活方式建议
                LifestyleAdvices = t.LifestyleAdvices.Select(l => new LifestyleAdviceDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Category = l.Category,
                    Rationale = l.Rationale,
                    Implementation = l.Implementation,
                    Frequency = l.Frequency,
                    Precautions = l.Precautions,
                    Benefits = l.Benefits,
                    Content = l.Content
                }).ToList(),

                // 饮食建议
                DietaryAdvices = t.DietaryAdvices.Select(d => new DietaryAdviceDto
                {
                    Id = d.Id,
                    Category = d.Category,
                    Title = d.Title,
                    DietaryPrinciples = d.DietaryPrinciples,
                    MealTiming = d.MealTiming,
                    CookingMethods = d.CookingMethods,
                    Rationale = d.Rationale,
                    SeasonalAdjustment = d.SeasonalAdjustment,
                    Precautions = d.Precautions,
                    RecommendedFoods = d.RecommendedFoods.Select(r => new RecommendedFoodDto
                    {
                        Id = r.Id,
                        FoodName = r.FoodName
                    }).ToList(),
                    AvoidedFoods = d.AvoidedFoods.Select(a => new AvoidedFoodDto
                    {
                        Id = a.Id,
                        FoodName = a.FoodName
                    }).ToList()
                }).ToList(),

                // 随访建议
                FollowUpAdvices = t.FollowUpAdvices.Select(f => new FollowUpAdviceDto
                {
                    Id = f.Id,
                    Title = f.Title,
                    Timing = f.Timing,
                    Purpose = f.Purpose,
                    PreparationRequired = f.PreparationRequired,
                    EmergencyConditions = f.EmergencyConditions,
                    SelfMonitoring = f.SelfMonitoring,
                    ContactInformation = f.ContactInformation,
                }).ToList(),

                // 中药安全警告
                HerbalWarnings = t.HerbalWarnings.Select(h => new HerbalWarningDto
                {
                    Id = h.Id,
                    WarningType = h.WarningType,
                    Title = h.Title,
                    Content = h.Content,
                    SeverityLevel = h.SeverityLevel,
                    SymptomsToWatch = h.SymptomsToWatch,
                    ActionRequired = h.ActionRequired,
                    PreventionMeasures = h.PreventionMeasures,
                    SpecialPopulations = h.SpecialPopulations,
                    AffectedMedications = h.AffectedMedications.Select(m => new AffectedMedicationDto
                    {
                        Id = m.Id,
                        MedicationName = m.MedicationName
                    }).ToList(),
                }).ToList(),

                // 食疗安全警告
                //DietaryWarnings = t.DietaryWarnings.Select(d => new DietaryWarningDto
                //{
                //    Id = d.Id,
                //    FoodName = d.FoodName,
                //    Warning = d.Warning
                //}).ToList()
            };
        }

        // 食疗警告
        private DietaryWarningDto DietaryWarningMapDto(DietaryWarningDto dietaryWarning)
        {
            return new DietaryWarningDto
            {
                Id = dietaryWarning.Id,
                WarningType = dietaryWarning.WarningType,
                Title = dietaryWarning.Title,
                Content = dietaryWarning.Content,
                SeverityLevel = dietaryWarning.SeverityLevel

            };
        }


        // JSON 反序列化方法，支持大小写不敏感
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private List<string> DeserializeList(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();

            return JsonSerializer.Deserialize<List<string>>(json, _jsonOptions) ?? new List<string>();
        }

        private T DeserializeObject<T>(string? json) where T : new()
        {
            if (string.IsNullOrWhiteSpace(json))
                return new T();

            return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
        }

        // 诊断概要审核
        //public async Task<IActionResult> OnPostSaveDiagnosisAsync()
        //{
        //    var entity = await _context.Syndromes
        //        .FirstOrDefaultAsync(s => s.SyndromeId == SyndromeId);

        //    if (entity == null)
        //    {
        //        TempData["Message"] = "医案不存在！";
        //        return RedirectToPage();
        //    }

        //    // 推荐方剂
        //    if (entity != null)
        //    {
        //        // ========== 1) 解析推荐方剂 ==========
        //        var recommendedFormulaslist = RecommendedFormulas?
        //            .Split(',', StringSplitOptions.RemoveEmptyEntries)
        //            .Select(s => s.Trim())
        //            .ToList() ?? new List<string>();

        //        // 覆盖 SyndromeDetail (内存中的 DTO)
        //        if (SyndromeDetail == null)
        //            SyndromeDetail = new SyndromeDetailDto();

        //        SyndromeDetail.RecommendedFormulas = recommendedFormulaslist;

        //        // 写回数据库：把推荐方剂序列化保存到 entity.RecommendedFormulas 字段
        //        entity.RecommendedFormulas = JsonSerializer.Serialize(recommendedFormulaslist);
        //    }

        //    // ========== 2) 更新 DiagnosisInfo ==========
        //    var diagnosis = DeserializeObject<DiagnosisInfo>(entity.DiagnosisInfo) ?? new DiagnosisInfo();

        //    diagnosis.Conclusion.PrimarySyndrome = PrimarySyndrome;

        //    diagnosis.Conclusion.AccompanyingSyndromes =
        //        string.IsNullOrEmpty(AccompanyingSyndromes)
        //            ? new List<string>()
        //            : AccompanyingSyndromes.Split(',').Select(x => x.Trim()).ToList();

        //    diagnosis.Conclusion.ConstitutionType = ConstitutionType;

        //    // 反序列化 DiagnosisInfo
        //    //var diagnosis = DeserializeObject<DiagnosisInfo>(entity.DiagnosisInfo) ?? new DiagnosisInfo();
        //    diagnosis.Analysis.AccompanyingBasis = AccompanyingBasis ?? string.Empty;

        //    entity.DiagnosisInfo = JsonSerializer.Serialize(diagnosis, new JsonSerializerOptions { WriteIndented = true });

        //    // 反序列化 TreatmentCareInfo
        //    var treatmentCare = DeserializeObject<TreatmentCareInfo>(entity.TreatmentCareInfo) ?? new TreatmentCareInfo();
        //    treatmentCare.TreatmentRecommendation.TreatmentPrinciple = TreatmentPrinciple ?? string.Empty;

        //    entity.TreatmentCareInfo = JsonSerializer.Serialize(treatmentCare, new JsonSerializerOptions { WriteIndented = true });

        //    // 写回 DiagnosisInfo JSON 字段
        //    entity.DiagnosisInfo = JsonSerializer.Serialize(
        //        diagnosis,
        //        new JsonSerializerOptions { WriteIndented = true }
        //    );

        //    entity.PathogenesisAnalysis = PathogenesisAnalysis ?? string.Empty;

        //    var parts = (TCMConclusion ?? "").Split('；', StringSplitOptions.RemoveEmptyEntries);
        //    if (parts.Length > 0)
        //        diagnosis.Analysis.PrimaryBasis = parts[0].Trim();
        //    if (parts.Length > 1)
        //        diagnosis.Analysis.ConstitutionInfluence = parts[1].Trim();
        //    entity.DiagnosisInfo = JsonSerializer.Serialize(diagnosis, new JsonSerializerOptions { WriteIndented = true });




        //    // 证候分类
        //    if (entity != null)
        //    {
        //        var syndromeCategoriesList = string.IsNullOrEmpty(SyndromeCategories)
        //                   ? new List<string>()
        //                   : SyndromeCategories.Split(',').ToList();
        //        entity.SyndromeCategories = JsonSerializer.Serialize(syndromeCategoriesList);
        //    }

        //    // 更新归属脏腑
        //    var organsList = string.IsNullOrEmpty(RelatedOrgans)
        //        ? new List<string>()
        //        : RelatedOrgans.Split(',').Select(s => s.Trim()).ToList();

        //    SyndromeDetail.RelatedOrgans = organsList;
        //    entity.RelatedOrgans = JsonSerializer.Serialize(organsList);

        //    // 常见疾病
        //    var diseasesList = string.IsNullOrEmpty(CommonDiseases)
        //        ? new List<string>()
        //        : CommonDiseases.Split(',').Select(s => s.Trim()).ToList();

        //    SyndromeDetail.CommonDiseases = diseasesList;
        //    entity.CommonDiseases = JsonSerializer.Serialize(diseasesList);

        //    // 主要症状
        //    var mainSymptomsList = string.IsNullOrEmpty(MainSymptoms)
        //        ? new List<string>()
        //        : MainSymptoms.Split(',', StringSplitOptions.RemoveEmptyEntries)
        //                      .Select(x => x.Trim()).ToList();
        //    entity.MainSymptoms = JsonSerializer.Serialize(mainSymptomsList);


        //    // ========== 3) 保存 ==========
        //    await _context.SaveChangesAsync();

        //    TempData["Message"] = "诊断概要已保存！";
        //    return RedirectToPage();
        //}

        // 审核通过
        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var treatment = await _context.Treatments.FindAsync(id);
            if (treatment == null) return NotFound();

            treatment.Status = TreatmentStatus.Finalized;
            //treatment.AuditorId = TreatmentPlan?.AuditorId;
            //treatment.AuditTime = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Message"] = "医案已通过审核！";
            return RedirectToPage("./Index"); // 刷新页面
        }

        // 驳回
        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var treatment = await _context.Treatments.FindAsync(id);
            if (treatment == null) return NotFound();

            treatment.Status = TreatmentStatus.CheckFailed;
            //treatment.AuditorId = TreatmentPlan?.AuditorId;
            //treatment.AuditTime = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Message"] = "医案已驳回！";
            return RedirectToPage("./Index"); // 刷新页面
        }

        //    public async Task<IActionResult> OnPostSaveDiagnosisAsync()
        //    {
        //        var entity = await _context.Syndromes
        //            .FirstOrDefaultAsync(s => s.SyndromeId == SyndromeId);

        //        if (entity == null)
        //        {
        //            TempData["Message"] = "医案不存在！";
        //            return RedirectToPage();
        //        }

        //        // 更新 DTO 的诊断信息
        //        var list = RecommendedFormulas?
        //.Split(',', StringSplitOptions.RemoveEmptyEntries)
        //.Select(s => s.Trim())
        //.ToList() ?? new List<string>(); // 更新方剂列表

        //        if (SyndromeDetail?.RecommendedFormulas != null)
        //        {
        //            SyndromeDetail.RecommendedFormulas = list;
        //        }
        //        else if (SyndromeDetail != null)
        //        {
        //            // 如果本身为空，重新构造
        //            SyndromeDetail.RecommendedFormulas = list;
        //        }

        //        var diagnosis = DeserializeObject<DiagnosisInfo>(entity.DiagnosisInfo) ?? new DiagnosisInfo();
        //        diagnosis.Conclusion.PrimarySyndrome = PrimarySyndrome;
        //        //diagnosis.Conclusion.AccompanyingSyndromes = AccompanyingSyndromes.Split(',').ToList();
        //        diagnosis.Conclusion.AccompanyingSyndromes =
        //string.IsNullOrEmpty(AccompanyingSyndromes)
        //    ? new List<string>()
        //    : AccompanyingSyndromes.Split(',').ToList();
        //        diagnosis.Conclusion.ConstitutionType = ConstitutionType;

        //        entity.DiagnosisInfo = JsonSerializer.Serialize(diagnosis, new JsonSerializerOptions { WriteIndented = true });

        //        await _context.SaveChangesAsync();

        //        TempData["Message"] = "诊断概要已保存！";
        //        return RedirectToPage();
        //    }

    }
}
