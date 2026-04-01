using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 预约信息表
    /// </summary>
    [Table("appointments")]
    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "患者GUID是必填项")]
        [StringLength(36)]
        public string PatientGuid { get; set; } = string.Empty;

        // 新增：关联诊所ID（支持多租户）
        [Required(ErrorMessage = "诊所ID是必填项")]
        public int ClinicId { get; set; }

        [Required(ErrorMessage = "医生ID是必填项")]
        public int DoctorId { get; set; }

        // 新增：医生姓名（显示用）
        [StringLength(50)]
        public string? DoctorName { get; set; }

        [Required(ErrorMessage = "预约时间是必填项")]
        public DateTime AppointmentTime { get; set; }

        // 新增：预约结束时间
        public DateTime EndTime { get; set; }

        // 新增：预约时长（分钟）
        [Range(15, 120, ErrorMessage = "预约时长应在15-120分钟之间")]
        public int Duration { get; set; } = 30;

        // 新增：预约类型（初诊、复诊等）
        [StringLength(20)]
        public string AppointmentType { get; set; } = "初诊";

        [Required(ErrorMessage = "状态是必填项")]
        [StringLength(10)]
        public string Status { get; set; } = "待就诊";

        // 新增：症状描述
        [StringLength(500)]
        public string? Symptoms { get; set; }

        // 新增：联系电话
        [StringLength(20)]
        public string? ContactPhone { get; set; }

        // 新增：备注
        [StringLength(1000)]
        public string? Remarks { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 新增：更新时间
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 新增：取消原因
        [StringLength(200)]
        public string? CancelReason { get; set; }

        // 业务逻辑方法
        public bool CanBeCancelled()
        {
            return Status == "待就诊" && AppointmentTime > DateTime.Now.AddHours(2);
        }

        public bool IsExpired()
        {
            return Status == "待就诊" && AppointmentTime < DateTime.Now;
        }

        public void CalculateEndTime()
        {
            EndTime = AppointmentTime.AddMinutes(Duration);
        }

        public bool IsTimeConflict(DateTime otherStart, DateTime otherEnd)
        {
            return (AppointmentTime < otherEnd) && (EndTime > otherStart);
        }
    }
}