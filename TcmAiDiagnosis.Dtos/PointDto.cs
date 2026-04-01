using System.ComponentModel.DataAnnotations;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 统一点位基础数据传输对象（通过type字段区分不同类型）
    /// </summary>
    public class PointDto
    {
        /// <summary>
        /// 点位ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联治疗方案ID
        /// </summary>
        public int TreatmentId { get; set; }

        /// <summary>
        /// 点位类型：Acupuncture（针灸）、Moxibustion（艾灸）、Cupping（拔罐）、Massage（推拿）
        /// </summary>
        [Required(ErrorMessage = "点位类型不能为空")]
        [StringLength(50, ErrorMessage = "点位类型长度不能超过50个字符")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 关联治疗ID（根据类型对应不同的治疗ID）
        /// </summary>
        public int RelatedTreatmentId { get; set; }

        /// <summary>
        /// 点位名称
        /// </summary>
        [Required(ErrorMessage = "点位名称不能为空")]
        [StringLength(200, ErrorMessage = "点位名称长度不能超过200个字符")]
        public string PointName { get; set; } = string.Empty;

        /// <summary>
        /// 点位位置
        /// </summary>
        [StringLength(200, ErrorMessage = "点位位置长度不能超过200个字符")]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// 点位坐标
        /// </summary>
        [StringLength(100, ErrorMessage = "点位坐标长度不能超过100个字符")]
        public string Coordinates { get; set; } = string.Empty;

        /// <summary>
        /// 解剖位置
        /// </summary>
        [StringLength(300, ErrorMessage = "解剖位置长度不能超过300个字符")]
        public string AnatomicalPosition { get; set; } = string.Empty;

        /// <summary>
        /// 操作方法
        /// </summary>
        [StringLength(200, ErrorMessage = "操作方法长度不能超过200个字符")]
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// 操作时长
        /// </summary>
        [StringLength(100, ErrorMessage = "操作时长长度不能超过100个字符")]
        public string Duration { get; set; } = string.Empty;

        /// <summary>
        /// 治疗频次
        /// </summary>
        [StringLength(100, ErrorMessage = "治疗频次长度不能超过100个字符")]
        public string Frequency { get; set; } = string.Empty;

        /// <summary>
        /// 预期疗效
        /// </summary>
        [StringLength(500, ErrorMessage = "预期疗效长度不能超过500个字符")]
        public string Efficacy { get; set; } = string.Empty;

        /// <summary>
        /// 适应症
        /// </summary>
        [StringLength(500, ErrorMessage = "适应症长度不能超过500个字符")]
        public string Indications { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 统一点位详情数据传输对象
    /// </summary>
    public class PointDetailDto : PointDto
    {
        /// <summary>
        /// 操作技法
        /// </summary>
        [StringLength(200, ErrorMessage = "操作技法长度不能超过200个字符")]
        public string Technique { get; set; } = string.Empty;

        /// <summary>
        /// 疗程
        /// </summary>
        [StringLength(100, ErrorMessage = "疗程长度不能超过100个字符")]
        public string CourseDuration { get; set; } = string.Empty;

        /// <summary>
        /// 操作要点
        /// </summary>
        [StringLength(500, ErrorMessage = "操作要点长度不能超过500个字符")]
        public string TechniquePoints { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        [StringLength(500, ErrorMessage = "注意事项长度不能超过500个字符")]
        public string Precautions { get; set; } = string.Empty;

        /// <summary>
        /// 禁忌症
        /// </summary>
        [StringLength(500, ErrorMessage = "禁忌症长度不能超过500个字符")]
        public string Contraindications { get; set; } = string.Empty;

        /// <summary>
        /// 治疗后护理
        /// </summary>
        [StringLength(500, ErrorMessage = "治疗后护理长度不能超过500个字符")]
        public string PostTreatmentCare { get; set; } = string.Empty;

        /// <summary>
        /// 操作说明
        /// </summary>
        [StringLength(500, ErrorMessage = "操作说明长度不能超过500个字符")]
        public string Instructions { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500, ErrorMessage = "备注长度不能超过500个字符")]
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// 创建人
        /// </summary>
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public int? UpdatedByUserId { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// 软删除标志
        /// </summary>
        public bool IsDeleted { get; set; }

        // 针灸特有字段
        /// <summary>
        /// 针具类型（仅针灸使用）
        /// </summary>
        [StringLength(100, ErrorMessage = "针具类型长度不能超过100个字符")]
        public string? NeedleType { get; set; }

        /// <summary>
        /// 针刺深度（仅针灸使用）
        /// </summary>
        [StringLength(100, ErrorMessage = "针刺深度长度不能超过100个字符")]
        public string? Depth { get; set; }

        // 艾灸特有字段
        /// <summary>
        /// 艾绒类型（仅艾灸使用）
        /// </summary>
        [StringLength(100, ErrorMessage = "艾绒类型长度不能超过100个字符")]
        public string? MoxaType { get; set; }

        /// <summary>
        /// 温度控制（仅艾灸使用）
        /// </summary>
        [StringLength(100, ErrorMessage = "温度控制长度不能超过100个字符")]
        public string? TemperatureControl { get; set; }

        // 拔罐特有字段
        /// <summary>
        /// 罐具规格（仅拔罐使用）
        /// </summary>
        [StringLength(100, ErrorMessage = "罐具规格长度不能超过100个字符")]
        public string? CupSize { get; set; }

        /// <summary>
        /// 吸力强度（仅拔罐使用）
        /// </summary>
        [StringLength(100, ErrorMessage = "吸力强度长度不能超过100个字符")]
        public string? SuctionStrength { get; set; }

        // 推拿特有字段
        /// <summary>
        /// 力度等级（仅推拿使用）
        /// </summary>
        [StringLength(100, ErrorMessage = "力度等级长度不能超过100个字符")]
        public string? PressureLevel { get; set; }
    }
}