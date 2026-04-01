using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    public class DietaryWarning
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TreatmentId { get; set; }

        [ForeignKey("TreatmentId")]
        public Treatment Treatment { get; set; }

        [Required]
        public string WarningType { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string SeverityLevel { get; set; }
    }
}