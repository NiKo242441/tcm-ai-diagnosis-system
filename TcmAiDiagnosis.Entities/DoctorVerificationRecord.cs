using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Entities
{
    public class DoctorVerificationRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TreatmentId { get; set; }

        [ForeignKey("TreatmentId")]
        public Treatment Treatment { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public User Doctor { get; set; }

        [Required]
        public string OperationType { get; set; }

        [Required]
        public string RiskLevel { get; set; }

        [Required]
        public string VerificationMethod { get; set; }

        [Required]
        public bool VerificationResult { get; set; }

        public string? Reason { get; set; }

        [Required]
        public ReviewStatus? ReviewStatus { get; set; }

        public int? ReviewerId { get; set; }

        [ForeignKey("ReviewerId")]
        public User? Reviewer { get; set; }

        public string? ReviewComments { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [Required]
        public string ClientIp { get; set; }

        [Required]
        public string UserAgent { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}