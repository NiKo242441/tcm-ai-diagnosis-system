using System.ComponentModel.DataAnnotations;

namespace TcmAiDiagnosis.Entities
{
    public class HighRiskOperationConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OperationName { get; set; }

        [Required]
        public string RiskCategory { get; set; }

        [Required]
        public string TriggerCondition { get; set; }

        [Required]
        public int VerificationLevel { get; set; }

        [Required]
        public string DefaultAction { get; set; }

        [Required]
        public string CustomMessage { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}