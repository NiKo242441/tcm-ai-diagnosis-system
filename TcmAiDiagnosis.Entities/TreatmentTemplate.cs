using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    public class TreatmentTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TemplateName { get; set; }

        public string? Description { get; set; }

        [Required]
        public string Scope { get; set; }

        public int? SourceTreatmentId { get; set; }

        [ForeignKey("SourceTreatmentId")]
        public Treatment? SourceTreatment { get; set; }

        [Required]
        public int UsageCount { get; set; }

        public string? TcmDiagnosis { get; set; }

        public string? SyndromeAnalysis { get; set; }

        public string? TreatmentPrinciple { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User CreatedByUser { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}