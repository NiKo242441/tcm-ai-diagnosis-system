using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Areas.Doctor.Pages.Visits
{
    public class TreatmentModel : PageModel
    {
        private readonly SyndromeDomain _syndromeDomain;

        public TreatmentModel(SyndromeDomain syndromeDomain)
        {
            _syndromeDomain = syndromeDomain;
        }

        [BindProperty(SupportsGet = true)]
        public int SyndromeId { get; set; }

        public Syndrome? Syndrome { get; set; }

        public List<string> RecommendedFormulas { get; set; } = new();
        public List<string> MainSymptoms { get; set; } = new();
        public List<string> CommonDiseases { get; set; } = new();
        public List<string> SyndromeCategories { get; set; } = new();
        public List<string> RelatedOrgans { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int visitId, int syndromeId)
        {
            SyndromeId = syndromeId;

            Syndrome = await _syndromeDomain.GetSyndromeByIdAsync(SyndromeId);

            if (Syndrome == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(Syndrome.RecommendedFormulas))
                RecommendedFormulas = JsonSerializer.Deserialize<List<string>>(Syndrome.RecommendedFormulas)!;

            if (!string.IsNullOrEmpty(Syndrome.MainSymptoms))
                MainSymptoms = JsonSerializer.Deserialize<List<string>>(Syndrome.MainSymptoms)!;

            if (!string.IsNullOrEmpty(Syndrome.CommonDiseases))
                CommonDiseases = JsonSerializer.Deserialize<List<string>>(Syndrome.CommonDiseases)!;

            if (!string.IsNullOrEmpty(Syndrome.SyndromeCategories))
                SyndromeCategories = JsonSerializer.Deserialize<List<string>>(Syndrome.SyndromeCategories)!;

            if (!string.IsNullOrEmpty(Syndrome.RelatedOrgans))
                RelatedOrgans = JsonSerializer.Deserialize<List<string>>(Syndrome.RelatedOrgans)!;

            return Page();
        }
    }
}