using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Web.Areas.Doctor.Pages.Visits
{
    public class TreatmentModel : PageModel
    {
        private readonly SyndromeDomain _syndromeDomain;
        private readonly TreatmentDomain _treatmentDomain;
        private readonly UserManager<User> _userManager;

        public TreatmentModel(
            SyndromeDomain syndromeDomain,
            TreatmentDomain treatmentDomain,
            UserManager<User> userManager)
        {
            _syndromeDomain = syndromeDomain;
            _treatmentDomain = treatmentDomain;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int SyndromeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int VisitId { get; set; }

        [BindProperty]
        public List<int> SelectedPrescriptionIds { get; set; } = new();

        [BindProperty]
        public List<int> SelectedAcupunctureIds { get; set; } = new();

        [BindProperty]
        public List<int> SelectedMoxibustionIds { get; set; } = new();

        [BindProperty]
        public List<int> SelectedCuppingIds { get; set; } = new();

        [BindProperty]
        public List<int> SelectedDietaryTherapyIds { get; set; } = new();

        [BindProperty]
        public List<int> SelectedLifestyleAdviceIds { get; set; } = new();

        [BindProperty]
        public List<int> SelectedDietaryAdviceIds { get; set; } = new();

        [BindProperty]
        public List<int> SelectedFollowUpAdviceIds { get; set; } = new();

        [BindProperty]
        public List<int> SelectedHerbalWarningIds { get; set; } = new();

        public Syndrome? Syndrome { get; set; }
        public TreatmentDto? Treatment { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int visitId, int syndromeId)
        {
            VisitId = visitId;
            SyndromeId = syndromeId;

            Syndrome = await _syndromeDomain.GetSyndromeByIdAsync(SyndromeId);

            if (Syndrome == null)
            {
                return NotFound();
            }

            await EnsureTreatmentLoadedAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostRegenerateAsync(int visitId, int syndromeId)
        {
            try
            {
                await _treatmentDomain.GenerateAndSaveAiTreatmentAsync(syndromeId);
                SuccessMessage = "AI治疗方案已重新生成。";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"重新生成治疗方案失败：{ex.Message}";
            }

            return RedirectToPage(new { visitId, syndromeId });
        }

        public async Task<IActionResult> OnPostConfirmAsync(int visitId, int syndromeId, int treatmentId)
        {
            VisitId = visitId;
            SyndromeId = syndromeId;

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var sourceTreatment = await _treatmentDomain.GetTreatmentByIdAsync(treatmentId)
                ?? await _treatmentDomain.GetLatestTreatmentBySyndromeIdAsync(syndromeId);

            if (sourceTreatment == null)
            {
                ErrorMessage = "未找到可确认的治疗方案。";
                return RedirectToPage(new { visitId, syndromeId });
            }

            var confirmedTreatment = BuildConfirmedTreatment(sourceTreatment);
            if (!HasConfirmedContent(confirmedTreatment))
            {
                ErrorMessage = "请至少选择一项要保存的治疗内容。";
                return RedirectToPage(new { visitId, syndromeId });
            }

            try
            {
                var newTreatmentId = await _treatmentDomain.CreateNewTreatmentVersionAsync(
                    confirmedTreatment,
                    currentUser.Id,
                    TreatmentDomain.VersionIncrementType.Minor);

                await _treatmentDomain.UpdateTreatmentStatusAsync(newTreatmentId, TreatmentStatus.Finalized, currentUser.Id);
                SuccessMessage = "已保存医生确认后的治疗方案。";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"保存确认方案失败：{ex.Message}";
            }

            return RedirectToPage(new { visitId, syndromeId });
        }

        public string GetStatusText()
        {
            return Treatment?.Status switch
            {
                TreatmentStatus.Generating => "生成中",
                TreatmentStatus.AIGenerated => "AI已生成",
                TreatmentStatus.Editing => "编辑中",
                TreatmentStatus.Checking => "安全检查中",
                TreatmentStatus.CheckFailed => "检查失败",
                TreatmentStatus.Versioning => "版本处理中",
                TreatmentStatus.Finalized => "已定稿",
                TreatmentStatus.Archived => "已归档",
                _ => "未生成"
            };
        }

        public string GetStatusBadgeClass()
        {
            return Treatment?.Status switch
            {
                TreatmentStatus.Generating => "bg-warning text-dark",
                TreatmentStatus.AIGenerated => "bg-success",
                TreatmentStatus.Editing => "bg-primary",
                TreatmentStatus.Checking => "bg-info text-dark",
                TreatmentStatus.CheckFailed => "bg-danger",
                TreatmentStatus.Versioning => "bg-secondary",
                TreatmentStatus.Finalized => "bg-dark",
                TreatmentStatus.Archived => "bg-secondary",
                _ => "bg-light text-dark"
            };
        }

        private async Task EnsureTreatmentLoadedAsync()
        {
            Treatment = await _treatmentDomain.GetLatestTreatmentBySyndromeIdAsync(SyndromeId);
            if (Treatment != null)
            {
                return;
            }

            try
            {
                await _treatmentDomain.GenerateAndSaveAiTreatmentAsync(SyndromeId);
                Treatment = await _treatmentDomain.GetLatestTreatmentBySyndromeIdAsync(SyndromeId);

                if (Treatment != null)
                {
                    SuccessMessage ??= "已生成最新 AI 治疗方案。";
                    return;
                }

                ErrorMessage ??= "治疗方案生成完成，但未能读取到结果。";
            }
            catch (Exception ex)
            {
                ErrorMessage ??= $"生成治疗方案失败：{ex.Message}";
            }
        }

        private TreatmentDto BuildConfirmedTreatment(TreatmentDto source)
        {
            return new TreatmentDto
            {
                Id = source.Id,
                PatientId = source.PatientId,
                VisitId = source.VisitId,
                SyndromeId = source.SyndromeId,
                Version = source.Version,
                Status = source.Status,
                TcmDiagnosis = source.TcmDiagnosis,
                SyndromeAnalysis = source.SyndromeAnalysis,
                TreatmentPrinciple = source.TreatmentPrinciple,
                ExpectedOutcome = source.ExpectedOutcome,
                Precautions = source.Precautions,
                Prescriptions = source.Prescriptions
                    .Where(p => SelectedPrescriptionIds.Contains(p.Id))
                    .ToList(),
                Acupunctures = source.Acupunctures
                    .Where(p => SelectedAcupunctureIds.Contains(p.Id))
                    .ToList(),
                Moxibustions = source.Moxibustions
                    .Where(p => SelectedMoxibustionIds.Contains(p.Id))
                    .ToList(),
                Cuppings = source.Cuppings
                    .Where(p => SelectedCuppingIds.Contains(p.Id))
                    .ToList(),
                DietaryTherapies = source.DietaryTherapies
                    .Where(p => SelectedDietaryTherapyIds.Contains(p.Id))
                    .ToList(),
                LifestyleAdvices = source.LifestyleAdvices
                    .Where(p => SelectedLifestyleAdviceIds.Contains(p.Id))
                    .ToList(),
                DietaryAdvices = source.DietaryAdvices
                    .Where(p => SelectedDietaryAdviceIds.Contains(p.Id))
                    .ToList(),
                FollowUpAdvices = source.FollowUpAdvices
                    .Where(p => SelectedFollowUpAdviceIds.Contains(p.Id))
                    .ToList(),
                HerbalWarnings = source.HerbalWarnings
                    .Where(p => SelectedHerbalWarningIds.Contains(p.Id))
                    .ToList(),
                DietaryWarnings = source.DietaryWarnings
            };
        }

        private static bool HasConfirmedContent(TreatmentDto treatment)
        {
            return treatment.Prescriptions.Any()
                || treatment.Acupunctures.Any()
                || treatment.Moxibustions.Any()
                || treatment.Cuppings.Any()
                || treatment.DietaryTherapies.Any()
                || treatment.LifestyleAdvices.Any()
                || treatment.DietaryAdvices.Any()
                || treatment.FollowUpAdvices.Any()
                || treatment.HerbalWarnings.Any();
        }
    }
}
