using System.ComponentModel.DataAnnotations;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 拔罐点位基础数据传输对象
    /// </summary>
    public class CuppingPointDto
    {
        /// <summary>
        /// 拔罐点位ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联拔罐ID
        /// </summary>
        public int CuppingId { get; set; }

        /// <summary>
        /// 穴位/部位名称
        /// </summary>
        [Required(ErrorMessage = "穴位/部位名称不能为空")]
        [StringLength(100, ErrorMessage = "穴位/部位名称长度不能超过100个字符")]
        public string PointName { get; set; } = string.Empty;

        /// <summary>
        /// 拔罐方法
        /// </summary>
        [StringLength(200, ErrorMessage = "拔罐方法长度不能超过200个字符")]
        public string CuppingMethod { get; set; } = string.Empty;

        /// <summary>
        /// 留罐时间
        /// </summary>
        [StringLength(50, ErrorMessage = "留罐时间长度不能超过50个字符")]
        public string RetentionTime { get; set; } = string.Empty;

        /// <summary>
        /// 罐具大小
        /// </summary>
        [StringLength(50, ErrorMessage = "罐具大小长度不能超过50个字符")]
        public string CupSize { get; set; } = string.Empty;

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
    /// 拔罐点位详情数据传输对象
    /// </summary>
    public class CuppingPointDetailDto : CuppingPointDto
    {
        /// <summary>
        /// 穴位/部位定位
        /// </summary>
        [StringLength(500, ErrorMessage = "穴位/部位定位长度不能超过500个字符")]
        public string PointLocation { get; set; } = string.Empty;

        /// <summary>
        /// 拔罐数量
        /// </summary>
        public int? CupCount { get; set; }

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
        /// 预期罐象
        /// </summary>
        [StringLength(500, ErrorMessage = "预期罐象长度不能超过500个字符")]
        public string ExpectedCuppingPattern { get; set; } = string.Empty;

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
    /// 创建拔罐点位请求DTO
    /// </summary>
    public class CuppingPointCreateDto
    {
        /// <summary>
        /// 关联拔罐ID
        /// </summary>
        [Required(ErrorMessage = "拔罐ID不能为空")]
        public int CuppingId { get; set; }

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
        /// 拔罐方法
        /// </summary>
        [StringLength(200, ErrorMessage = "拔罐方法长度不能超过200个字符")]
        public string CuppingMethod { get; set; } = string.Empty;

        /// <summary>
        /// 留罐时间
        /// </summary>
        [StringLength(50, ErrorMessage = "留罐时间长度不能超过50个字符")]
        public string RetentionTime { get; set; } = string.Empty;

        /// <summary>
        /// 罐具大小
        /// </summary>
        [StringLength(50, ErrorMessage = "罐具大小长度不能超过50个字符")]
        public string CupSize { get; set; } = string.Empty;

        /// <summary>
        /// 拔罐数量
        /// </summary>
        public int? CupCount { get; set; }

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
        /// 预期罐象
        /// </summary>
        [StringLength(500, ErrorMessage = "预期罐象长度不能超过500个字符")]
        public string ExpectedCuppingPattern { get; set; } = string.Empty;

        /// <summary>
        /// 租户ID
        /// </summary>
        [Required(ErrorMessage = "租户ID不能为空")]
        public int TenantId { get; set; }
    }

    /// <summary>
    /// 更新拔罐点位请求DTO
    /// </summary>
    public class CuppingPointUpdateDto
    {
        /// <summary>
        /// 拔罐点位ID
        /// </summary>
        [Required(ErrorMessage = "拔罐点位ID不能为空")]
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
        /// 拔罐方法
        /// </summary>
        [StringLength(200, ErrorMessage = "拔罐方法长度不能超过200个字符")]
        public string CuppingMethod { get; set; } = string.Empty;

        /// <summary>
        /// 留罐时间
        /// </summary>
        [StringLength(50, ErrorMessage = "留罐时间长度不能超过50个字符")]
        public string RetentionTime { get; set; } = string.Empty;

        /// <summary>
        /// 罐具大小
        /// </summary>
        [StringLength(50, ErrorMessage = "罐具大小长度不能超过50个字符")]
        public string CupSize { get; set; } = string.Empty;

        /// <summary>
        /// 拔罐数量
        /// </summary>
        public int? CupCount { get; set; }

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
        /// 预期罐象
        /// </summary>
        [StringLength(500, ErrorMessage = "预期罐象长度不能超过500个字符")]
        public string ExpectedCuppingPattern { get; set; } = string.Empty;
    }
}