using System.ComponentModel.DataAnnotations;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 推拿点位基础数据传输对象
    /// </summary>
    public class MassagePointDto
    {
        /// <summary>
        /// 推拿点位ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联推拿ID
        /// </summary>
        public int MassageId { get; set; }

        /// <summary>
        /// 穴位/部位名称
        /// </summary>
        [Required(ErrorMessage = "穴位/部位名称不能为空")]
        [StringLength(100, ErrorMessage = "穴位/部位名称长度不能超过100个字符")]
        public string PointName { get; set; } = string.Empty;

        /// <summary>
        /// 推拿手法
        /// </summary>
        [StringLength(200, ErrorMessage = "推拿手法长度不能超过200个字符")]
        public string MassageTechnique { get; set; } = string.Empty;

        /// <summary>
        /// 操作时长
        /// </summary>
        [StringLength(50, ErrorMessage = "操作时长长度不能超过50个字符")]
        public string OperationDuration { get; set; } = string.Empty;

        /// <summary>
        /// 力量强度
        /// </summary>
        [StringLength(50, ErrorMessage = "力量强度长度不能超过50个字符")]
        public string ForceIntensity { get; set; } = string.Empty;

        /// <summary>
        /// 操作次数
        /// </summary>
        public int? OperationCount { get; set; }

        /// <summary>
        /// 治疗作用
        /// </summary>
        [StringLength(500, ErrorMessage = "治疗作用长度不能超过500个字符")]
        public string TherapeuticEffect { get; set; } = string.Empty;

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
    /// 推拿点位详情数据传输对象
    /// </summary>
    public class MassagePointDetailDto : MassagePointDto
    {
        /// <summary>
        /// 穴位/部位定位
        /// </summary>
        [StringLength(500, ErrorMessage = "穴位/部位定位长度不能超过500个字符")]
        public string PointLocation { get; set; } = string.Empty;

        /// <summary>
        /// 操作频率
        /// </summary>
        [StringLength(50, ErrorMessage = "操作频率长度不能超过50个字符")]
        public string OperationFrequency { get; set; } = string.Empty;

        /// <summary>
        /// 特殊操作
        /// </summary>
        [StringLength(200, ErrorMessage = "特殊操作长度不能超过200个字符")]
        public string SpecialOperation { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        [StringLength(500, ErrorMessage = "注意事项长度不能超过500个字符")]
        public string Precautions { get; set; } = string.Empty;

        /// <summary>
        /// 预期反应
        /// </summary>
        [StringLength(500, ErrorMessage = "预期反应长度不能超过500个字符")]
        public string ExpectedReaction { get; set; } = string.Empty;

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
    }

    /// <summary>
    /// 创建推拿点位请求DTO
    /// </summary>
    public class MassagePointCreateDto
    {
        /// <summary>
        /// 关联推拿ID
        /// </summary>
        [Required(ErrorMessage = "推拿ID不能为空")]
        public int MassageId { get; set; }

        /// <summary>
        /// 穴位/部位名称
        /// </summary>
        [Required(ErrorMessage = "穴位/部位名称不能为空")]
        [StringLength(100, ErrorMessage = "穴位/部位名称长度不能超过100个字符")]
        public string PointName { get; set; } = string.Empty;

        /// <summary>
        /// 穴位/部位定位
        /// </summary>
        [StringLength(500, ErrorMessage = "穴位/部位定位长度不能超过500个字符")]
        public string PointLocation { get; set; } = string.Empty;

        /// <summary>
        /// 推拿手法
        /// </summary>
        [StringLength(200, ErrorMessage = "推拿手法长度不能超过200个字符")]
        public string MassageTechnique { get; set; } = string.Empty;

        /// <summary>
        /// 操作时长
        /// </summary>
        [StringLength(50, ErrorMessage = "操作时长长度不能超过50个字符")]
        public string OperationDuration { get; set; } = string.Empty;

        /// <summary>
        /// 力量强度
        /// </summary>
        [StringLength(50, ErrorMessage = "力量强度长度不能超过50个字符")]
        public string ForceIntensity { get; set; } = string.Empty;

        /// <summary>
        /// 操作频率
        /// </summary>
        [StringLength(50, ErrorMessage = "操作频率长度不能超过50个字符")]
        public string OperationFrequency { get; set; } = string.Empty;

        /// <summary>
        /// 操作次数
        /// </summary>
        public int? OperationCount { get; set; }

        /// <summary>
        /// 特殊操作
        /// </summary>
        [StringLength(200, ErrorMessage = "特殊操作长度不能超过200个字符")]
        public string SpecialOperation { get; set; } = string.Empty;

        /// <summary>
        /// 治疗作用
        /// </summary>
        [StringLength(500, ErrorMessage = "治疗作用长度不能超过500个字符")]
        public string TherapeuticEffect { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        [StringLength(500, ErrorMessage = "注意事项长度不能超过500个字符")]
        public string Precautions { get; set; } = string.Empty;

        /// <summary>
        /// 预期反应
        /// </summary>
        [StringLength(500, ErrorMessage = "预期反应长度不能超过500个字符")]
        public string ExpectedReaction { get; set; } = string.Empty;

        /// <summary>
        /// 租户ID
        /// </summary>
        [Required(ErrorMessage = "租户ID不能为空")]
        public int TenantId { get; set; }
    }

    /// <summary>
    /// 更新推拿点位请求DTO
    /// </summary>
    public class MassagePointUpdateDto
    {
        /// <summary>
        /// 推拿点位ID
        /// </summary>
        [Required(ErrorMessage = "推拿点位ID不能为空")]
        public int Id { get; set; }

        /// <summary>
        /// 穴位/部位名称
        /// </summary>
        [Required(ErrorMessage = "穴位/部位名称不能为空")]
        [StringLength(100, ErrorMessage = "穴位/部位名称长度不能超过100个字符")]
        public string PointName { get; set; } = string.Empty;

        /// <summary>
        /// 穴位/部位定位
        /// </summary>
        [StringLength(500, ErrorMessage = "穴位/部位定位长度不能超过500个字符")]
        public string PointLocation { get; set; } = string.Empty;

        /// <summary>
        /// 推拿手法
        /// </summary>
        [StringLength(200, ErrorMessage = "推拿手法长度不能超过200个字符")]
        public string MassageTechnique { get; set; } = string.Empty;

        /// <summary>
        /// 操作时长
        /// </summary>
        [StringLength(50, ErrorMessage = "操作时长长度不能超过50个字符")]
        public string OperationDuration { get; set; } = string.Empty;

        /// <summary>
        /// 力量强度
        /// </summary>
        [StringLength(50, ErrorMessage = "力量强度长度不能超过50个字符")]
        public string ForceIntensity { get; set; } = string.Empty;

        /// <summary>
        /// 操作频率
        /// </summary>
        [StringLength(50, ErrorMessage = "操作频率长度不能超过50个字符")]
        public string OperationFrequency { get; set; } = string.Empty;

        /// <summary>
        /// 操作次数
        /// </summary>
        public int? OperationCount { get; set; }

        /// <summary>
        /// 特殊操作
        /// </summary>
        [StringLength(200, ErrorMessage = "特殊操作长度不能超过200个字符")]
        public string SpecialOperation { get; set; } = string.Empty;

        /// <summary>
        /// 治疗作用
        /// </summary>
        [StringLength(500, ErrorMessage = "治疗作用长度不能超过500个字符")]
        public string TherapeuticEffect { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        [StringLength(500, ErrorMessage = "注意事项长度不能超过500个字符")]
        public string Precautions { get; set; } = string.Empty;

        /// <summary>
        /// 预期反应
        /// </summary>
        [StringLength(500, ErrorMessage = "预期反应长度不能超过500个字符")]
        public string ExpectedReaction { get; set; } = string.Empty;
    }
}