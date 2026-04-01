using System.ComponentModel.DataAnnotations;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 推拿治疗基础数据传输对象
    /// </summary>
    public class MassageDto
    {
        /// <summary>
        /// 推拿ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联治疗方案ID
        /// </summary>
        public int TreatmentId { get; set; }

        /// <summary>
        /// 推拿名称
        /// </summary>
        [Required(ErrorMessage = "推拿名称不能为空")]
        [StringLength(200, ErrorMessage = "推拿名称长度不能超过200个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 推拿分类
        /// </summary>
        [StringLength(100, ErrorMessage = "推拿分类长度不能超过100个字符")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 详细描述
        /// </summary>
        [StringLength(1000, ErrorMessage = "详细描述长度不能超过1000个字符")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 操作技法
        /// </summary>
        [StringLength(200, ErrorMessage = "操作技法长度不能超过200个字符")]
        public string Techniques { get; set; } = string.Empty;

        /// <summary>
        /// 治疗时长
        /// </summary>
        [StringLength(100, ErrorMessage = "治疗时长长度不能超过100个字符")]
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
        /// 是否AI生成
        /// </summary>
        public bool IsAiOriginated { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public TreatmentStatus Status { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 是否最新版本
        /// </summary>
        public bool IsLatest { get; set; }

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
    /// 推拿治疗详情数据传输对象
    /// </summary>
    public class MassageDetailDto : MassageDto
    {
        /// <summary>
        /// 适用人群
        /// </summary>
        [StringLength(500, ErrorMessage = "适用人群长度不能超过500个字符")]
        public string SuitableFor { get; set; } = string.Empty;

        /// <summary>
        /// 禁忌症
        /// </summary>
        [StringLength(500, ErrorMessage = "禁忌症长度不能超过500个字符")]
        public string Contraindications { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        [StringLength(500, ErrorMessage = "注意事项长度不能超过500个字符")]
        public string Precautions { get; set; } = string.Empty;

        /// <summary>
        /// 操作要点
        /// </summary>
        [StringLength(500, ErrorMessage = "操作要点长度不能超过500个字符")]
        public string KeyPoints { get; set; } = string.Empty;

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
        /// 推拿点位列表
        /// </summary>
        public List<MassagePointDto> MassagePoints { get; set; } = new List<MassagePointDto>();
    }

    /// <summary>
    /// 创建推拿治疗请求DTO
    /// </summary>
    public class MassageCreateDto
    {
        /// <summary>
        /// 关联治疗方案ID
        /// </summary>
        [Required(ErrorMessage = "治疗方案ID不能为空")]
        public int TreatmentId { get; set; }

        /// <summary>
        /// 推拿名称
        /// </summary>
        [Required(ErrorMessage = "推拿名称不能为空")]
        [StringLength(200, ErrorMessage = "推拿名称长度不能超过200个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 推拿分类
        /// </summary>
        [StringLength(100, ErrorMessage = "推拿分类长度不能超过100个字符")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 详细描述
        /// </summary>
        [StringLength(1000, ErrorMessage = "详细描述长度不能超过1000个字符")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 操作技法
        /// </summary>
        [StringLength(200, ErrorMessage = "操作技法长度不能超过200个字符")]
        public string Techniques { get; set; } = string.Empty;

        /// <summary>
        /// 治疗时长
        /// </summary>
        [StringLength(100, ErrorMessage = "治疗时长长度不能超过100个字符")]
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
        /// 适用人群
        /// </summary>
        [StringLength(500, ErrorMessage = "适用人群长度不能超过500个字符")]
        public string SuitableFor { get; set; } = string.Empty;

        /// <summary>
        /// 禁忌症
        /// </summary>
        [StringLength(500, ErrorMessage = "禁忌症长度不能超过500个字符")]
        public string Contraindications { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        [StringLength(500, ErrorMessage = "注意事项长度不能超过500个字符")]
        public string Precautions { get; set; } = string.Empty;

        /// <summary>
        /// 操作要点
        /// </summary>
        [StringLength(500, ErrorMessage = "操作要点长度不能超过500个字符")]
        public string KeyPoints { get; set; } = string.Empty;

        /// <summary>
        /// 是否AI生成
        /// </summary>
        public bool IsAiOriginated { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        [Required(ErrorMessage = "租户ID不能为空")]
        public int TenantId { get; set; }

        /// <summary>
        /// 推拿点位列表
        /// </summary>
        public List<MassagePointCreateDto> MassagePoints { get; set; } = new List<MassagePointCreateDto>();
    }

    /// <summary>
    /// 更新推拿治疗请求DTO
    /// </summary>
    public class MassageUpdateDto
    {
        /// <summary>
        /// 推拿ID
        /// </summary>
        [Required(ErrorMessage = "推拿ID不能为空")]
        public int Id { get; set; }

        /// <summary>
        /// 推拿名称
        /// </summary>
        [Required(ErrorMessage = "推拿名称不能为空")]
        [StringLength(200, ErrorMessage = "推拿名称长度不能超过200个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 推拿分类
        /// </summary>
        [StringLength(100, ErrorMessage = "推拿分类长度不能超过100个字符")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 详细描述
        /// </summary>
        [StringLength(1000, ErrorMessage = "详细描述长度不能超过1000个字符")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 操作技法
        /// </summary>
        [StringLength(200, ErrorMessage = "操作技法长度不能超过200个字符")]
        public string Techniques { get; set; } = string.Empty;

        /// <summary>
        /// 治疗时长
        /// </summary>
        [StringLength(100, ErrorMessage = "治疗时长长度不能超过100个字符")]
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
        /// 适用人群
        /// </summary>
        [StringLength(500, ErrorMessage = "适用人群长度不能超过500个字符")]
        public string SuitableFor { get; set; } = string.Empty;

        /// <summary>
        /// 禁忌症
        /// </summary>
        [StringLength(500, ErrorMessage = "禁忌症长度不能超过500个字符")]
        public string Contraindications { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        [StringLength(500, ErrorMessage = "注意事项长度不能超过500个字符")]
        public string Precautions { get; set; } = string.Empty;

        /// <summary>
        /// 操作要点
        /// </summary>
        [StringLength(500, ErrorMessage = "操作要点长度不能超过500个字符")]
        public string KeyPoints { get; set; } = string.Empty;

        /// <summary>
        /// 状态
        /// </summary>
        public TreatmentStatus Status { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 是否最新版本
        /// </summary>
        public bool IsLatest { get; set; }
    }
}