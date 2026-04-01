//using TcmDatabase.Enums; // Assuming your enums are in this namespace

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 就诊系列表 (用于跟踪一个患者针对某个主诉的多次就诊)
    /// </summary>
    public class VisitSeries
    {
        /// <summary>
        /// 系列ID
        /// </summary>
        public int SeriesId { get; set; }

        /// <summary>
        /// 患者用户ID (外键)
        /// </summary>
        public int PatientUserId { get; set; }

        /// <summary>
        /// 医生用户ID (外键)
        /// </summary>
        public int DoctorUserId { get; set; }

        /// <summary>
        /// 租户ID (外键)
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// 初次主诉
        /// </summary>
        public string? ChiefComplaint { get; set; }

        /// <summary>
        /// 患者信息描述
        /// </summary>
        public string? PatientNotes { get; set; }

        /// <summary>
        /// 下次随访时间 (医生建议的下次随访日期)
        /// </summary>
        public DateTime? NextFollowUpDate { get; set; }

        /// <summary>
        /// 下次复诊时间 (医生建议的下次复诊日期)
        /// </summary>
        public DateTime? NextFollowUpVisitDate { get; set; }
        /// <summary>
        /// 系列开始日期
        /// </summary>
        public DateTime SeriesStartDate { get; set; }

        /// <summary>
        /// 系列结束日期 (可空，表示尚未结束)
        /// </summary>
        public DateTime? SeriesEndDate { get; set; }

        /// <summary>
        /// 当前状态 (0进行中, 1已结束)
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }


        // 导航属性
        public User PatientUser { get; set; }
        public User DoctorUser { get; set; }
        public Tenant Tenant { get; set; }
        public ICollection<Visit> Visits { get; set; }
    }
}