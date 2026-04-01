using System;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 医生-患者关联表（多对多中间表）
    /// </summary>
    public class DoctorPatient
    {
        public int DoctorUserId { get; set; }
        public int PatientUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // 导航属性
        public User DoctorUser { get; set; }
        public User PatientUser { get; set; }
    }
}