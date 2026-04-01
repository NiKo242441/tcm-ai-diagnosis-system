//using TcmDatabase.Enums; // Assuming your enums are in this namespace

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 就诊记录表
    /// </summary>
    public class Visit
    {
        /// <summary>
        /// 就诊ID
        /// </summary>
        public int VisitId { get; set; }

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
        /// 就诊系列ID (外键, 可空)
        /// </summary>
        public int? SeriesId { get; set; }

        /// <summary>
        /// 在系列中的就诊顺序
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// 就诊日期时间
        /// </summary>
        public DateTime VisitDate { get; set; }

        /// <summary>
        /// 就诊类型 (初诊, 复诊, 随访)
        /// </summary>
        public VisitType VisitType { get; set; }

        /// <summary>
        /// 主诉描述
        /// </summary>
        public string? ChiefComplaint { get; set; }

        /// <summary>
        /// 伴随症状 (如发热、恶心、乏力等)
        /// </summary>
        public string? AccompanyingSymptoms { get; set; }

        /// <summary>
        /// 既往治疗效果 (如中药/西药/针灸效果)
        /// </summary>
        public string? PreviousTreatmentEffect { get; set; }

        /// <summary>
        /// 发病天数
        /// </summary>
        public int? OnsetDays { get; set; }

        /// <summary>
        /// 加重时间（如晨起、午后等）
        /// </summary>
        public string? AggravationTime { get; set; }

        /// <summary>
        /// 气候关系（如寒冷加重、炎热加重等）
        /// </summary>
        public string? ClimateRelationship { get; set; }

        /// <summary>
        /// 舌质
        /// </summary>
        public string? TongueQuality { get; set; }

        /// <summary>
        /// 舌苔
        /// </summary>
        public string? TongueCoating { get; set; }

        /// <summary>
        /// 舌形
        /// </summary>
        public string? TongueShape { get; set; }

        /// <summary>
        /// 舌咽症状
        /// </summary>
        public string? TongueSymptoms { get; set; }

        /// <summary>
        /// 脉象类型
        /// </summary>
        public string? PulseType { get; set; }

        /// <summary>
        /// 脉象特征
        /// </summary>
        public string? PulseFeatures { get; set; }

        /// <summary>
        /// 面色
        /// </summary>
        public string? FaceColor { get; set; }

        /// <summary>
        /// 精神状态
        /// </summary>
        public string? MentalState { get; set; }

        /// <summary>
        /// 饮食 (如喜热饮/冷饮、食欲变化)
        /// </summary>
        public string? Diet { get; set; }

        /// <summary>
        /// 睡眠 (入睡困难/早醒)
        /// </summary>
        public string? Sleep { get; set; }

        /// <summary>
        /// 大小便 (频率、颜色、质地)
        /// </summary>
        public string? Excretion { get; set; }

        /// <summary>
        /// 情绪状态 (焦虑/抑郁/平静)
        /// </summary>
        public string? EmotionState { get; set; }

        /// <summary>
        /// 汗出情况
        /// </summary>
        public string? Sweating { get; set; }

        /// <summary>
        /// 其他描述
        /// </summary>
        public string? OtherDescription { get; set; }

        /// <summary>
        /// 体质评估
        /// </summary>
        public string? ConstitutionAssessment { get; set; }

        /// <summary>
        /// 下次随访时间 (医生建议的下次随访日期)
        /// </summary>
        public DateTime? NextFollowUpDate { get; set; }

        /// <summary>
        /// 下次复诊时间 (医生建议的下次复诊日期)
        /// </summary>
        public DateTime? NextFollowUpVisitDate { get; set; }

        /// <summary>
        /// 需观察的症状
        /// </summary>
        public string? SymptomsToMonitor { get; set; }

        /// <summary>
        /// 服药反应跟踪
        /// </summary>
        public string? MedicationReactionTracking { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 文本描述
        /// </summary>
        public string VisitNotes { get; set; } = string.Empty;

        // 导航属性
        public User PatientUser { get; set; }
        public User DoctorUser { get; set; }
        public Tenant Tenant { get; set; }
        public VisitSeries Series { get; set; }
    }

    /// <summary>
    /// 就诊类型
    /// </summary>
    public enum VisitType
    {
        /// <summary>
        /// 初诊
        /// </summary>
        Initial,
        /// <summary>
        /// 复诊
        /// </summary>
        FollowUp,
        /// <summary>
        /// 随访
        /// </summary>
        FollowUpCall
    }
}