using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 患者信息表
    /// </summary>
    [Table("patients")]
    public class Patient
    {
        [Key]
        [StringLength(36)]
        public string PatientGuid { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(11)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(500)]
        public string? AllergyHistory { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
