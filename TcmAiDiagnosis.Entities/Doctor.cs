using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcmAiDiagnosis.Entities
{
    ///// <summary>
    ///// 医生信息表
    ///// </summary>
    [Table("doctors")]
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Title { get; set; }

        [StringLength(200)]
        public string? Specialty { get; set; }

        [Required]
        public int ClinicId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
