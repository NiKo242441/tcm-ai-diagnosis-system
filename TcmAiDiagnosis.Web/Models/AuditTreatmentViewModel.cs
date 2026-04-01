using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Web.Models
{
    public class AuditTreatmentViewModel
    {
        public Treatment TreatmentPlan { get; set; } = new();
        public SyndromeDetailDto? SyndromeDetail { get; set; }

        // 审核相关属性（这些不会保存到数据库，只用于页面显示和操作）
        public AuditStatus AuditStatus { get; set; } = AuditStatus.Pending;
        public int? AuditorId { get; set; }
        public DateTime? AuditTime { get; set; }
        public string? AuditComments { get; set; }

        // 页面绑定属性
        public int SyndromeId { get; set; }
        public string PrimarySyndrome { get; set; } = string.Empty;
        public string AccompanyingSyndromes { get; set; } = string.Empty;
        public string ConstitutionType { get; set; } = string.Empty;
        public List<string> SelectedFormulas { get; set; } = new();
        public string RecommendedFormulas { get; set; } = string.Empty;
        public string SyndromeCategories { get; set; } = string.Empty;
        public string RelatedOrgans { get; set; } = string.Empty;
        public string CommonDiseases { get; set; } = string.Empty;
        public string MainSymptoms { get; set; } = string.Empty;
        public string PathogenesisAnalysis { get; set; } = string.Empty;
        public string TCMConclusion { get; set; } = string.Empty;
        public string AccompanyingBasis { get; set; } = string.Empty;
        public string TreatmentPrinciple { get; set; } = string.Empty;
    }
}