using System.ComponentModel.DataAnnotations;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 针灸点位基础数据传输对象
    /// </summary>
    public class AcupuncturePointDto
    {
        /// <summary>
        /// 针灸点位ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联针灸ID
        /// </summary>
        public int AcupunctureId { get; set; }

        /// <summary>
        /// 穴位名称
        /// </summary>
        [Required(ErrorMessage = "穴位名称不能为空")]
        [StringLength(100, ErrorMessage = "穴位名称长度不能超过100个字符")]
        public string PointName { get; set; } = string.Empty;

        /// <summary>
        /// 穴位编号
        /// </summary>
        [StringLength(50, ErrorMessage = "穴位编号长度不能超过50个字符")]
        public string PointCode { get; set; } = string.Empty;

        /// <summary>
        /// 针刺方法
        /// </summary>
        [StringLength(200, ErrorMessage = "针刺方法长度不能超过200个字符")]
        public string NeedlingMethod { get; set; } = string.Empty;

        /// <summary>
        /// 进针深度
        /// </summary>
        [StringLength(50, ErrorMessage = "进针深度长度不能超过50个字符")]
        public string NeedlingDepth { get; set; } = string.Empty;

        /// <summary>
        /// 留针时间
        /// </summary>
        [StringLength(50, ErrorMessage = "留针时间长度不能超过50个字符")]
        public string RetentionTime { get; set; } = string.Empty;

        /// <summary>
        /// 刺激强度
        /// </summary>
        [StringLength(50, ErrorMessage = "刺激强度长度不能超过50个字符")]
        public string StimulationIntensity { get; set; } = string.Empty;

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
    /// 针灸点位详情数据传输对象
    /// </summary>
    public class AcupuncturePointDetailDto : AcupuncturePointDto
    {
        /// <summary>
        /// 穴位定位
        /// </summary>
        [StringLength(500, ErrorMessage = "穴位定位长度不能超过500个字符")]
        public string PointLocation { get; set; } = string.Empty;

        /// <summary>
        /// 穴位归经
        /// </summary>
        [StringLength(100, ErrorMessage = "穴位归经长度不能超过100个字符")]
        public string Meridian { get; set; } = string.Empty;

        /// <summary>
        /// 针刺角度
        /// </summary>
        [StringLength(50, ErrorMessage = "针刺角度长度不能超过50个字符")]
        public string NeedlingAngle { get; set; } = string.Empty;

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
        /// 关联穴位
        /// </summary>
        [StringLength(200, ErrorMessage = "关联穴位长度不能超过200个字符")]
        public string AssociatedPoints { get; set; } = string.Empty;

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
    /// 创建针灸点位请求DTO
    /// </summary>
    public class AcupuncturePointCreateDto
    {
        /// <summary>
        /// 关联针灸ID
        /// </summary>
        [Required(ErrorMessage = "针灸ID不能为空")]
        public int AcupunctureId { get; set; }

        /// <summary>
        /// 穴位名称
        /// </summary>
        [Required(ErrorMessage = "穴位名称不能为空")]
        [StringLength(100, ErrorMessage = "穴位名称长度不能超过100个字符")]
        public string PointName { get; set; } = string.Empty;

        /// <summary>
        /// 穴位编号
        /// </summary>
        [StringLength(50, ErrorMessage = "穴位编号长度不能超过50个字符")]
        public string PointCode { get; set; } = string.Empty;

        /// <summary>
        /// 穴位定位
        /// </summary>
        [StringLength(500, ErrorMessage = "穴位定位长度不能超过500个字符")]
        public string PointLocation { get; set; } = string.Empty;

        /// <summary>
        /// 穴位归经
        /// </summary>
        [StringLength(100, ErrorMessage = "穴位归经长度不能超过100个字符")]
        public string Meridian { get; set; } = string.Empty;

        /// <summary>
        /// 针刺方法
        /// </summary>
        [StringLength(200, ErrorMessage = "针刺方法长度不能超过200个字符")]
        public string NeedlingMethod { get; set; } = string.Empty;

        /// <summary>
        /// 针刺角度
        /// </summary>
        [StringLength(50, ErrorMessage = "针刺角度长度不能超过50个字符")]
        public string NeedlingAngle { get; set; } = string.Empty;

        /// <summary>
        /// 进针深度
        /// </summary>
        [StringLength(50, ErrorMessage = "进针深度长度不能超过50个字符")]
        public string NeedlingDepth { get; set; } = string.Empty;

        /// <summary>
        /// 留针时间
        /// </summary>
        [StringLength(50, ErrorMessage = "留针时间长度不能超过50个字符")]
        public string RetentionTime { get; set; } = string.Empty;

        /// <summary>
        /// 刺激强度
        /// </summary>
        [StringLength(50, ErrorMessage = "刺激强度长度不能超过50个字符")]
        public string StimulationIntensity { get; set; } = string.Empty;

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
        /// 关联穴位
        /// </summary>
        [StringLength(200, ErrorMessage = "关联穴位长度不能超过200个字符")]
        public string AssociatedPoints { get; set; } = string.Empty;

        /// <summary>
        /// 租户ID
        /// </summary>
        [Required(ErrorMessage = "租户ID不能为空")]
        public int TenantId { get; set; }
    }

    /// <summary>
    /// 更新针灸点位请求DTO
    /// </summary>
    public class AcupuncturePointUpdateDto
    {
        /// <summary>
        /// 针灸点位ID
        /// </summary>
        [Required(ErrorMessage = "针灸点位ID不能为空")]
        public int Id { get; set; }

        /// <summary>
        /// 穴位名称
        /// </summary>
        [Required(ErrorMessage = "穴位名称不能为空")]
        [StringLength(100, ErrorMessage = "穴位名称长度不能超过100个字符")]
        public string PointName { get; set; } = string.Empty;

        /// <summary>
        /// 穴位编号
        /// </summary>
        [StringLength(50, ErrorMessage = "穴位编号长度不能超过50个字符")]
        public string PointCode { get; set; } = string.Empty;

        /// <summary>
        /// 穴位定位
        /// </summary>
        [StringLength(500, ErrorMessage = "穴位定位长度不能超过500个字符")]
        public string PointLocation { get; set; } = string.Empty;

        /// <summary>
        /// 穴位归经
        /// </summary>
        [StringLength(100, ErrorMessage = "穴位归经长度不能超过100个字符")]
        public string Meridian { get; set; } = string.Empty;

        /// <summary>
        /// 针刺方法
        /// </summary>
        [StringLength(200, ErrorMessage = "针刺方法长度不能超过200个字符")]
        public string NeedlingMethod { get; set; } = string.Empty;

        /// <summary>
        /// 针刺角度
        /// </summary>
        [StringLength(50, ErrorMessage = "针刺角度长度不能超过50个字符")]
        public string NeedlingAngle { get; set; } = string.Empty;

        /// <summary>
        /// 进针深度
        /// </summary>
        [StringLength(50, ErrorMessage = "进针深度长度不能超过50个字符")]
        public string NeedlingDepth { get; set; } = string.Empty;

        /// <summary>
        /// 留针时间
        /// </summary>
        [StringLength(50, ErrorMessage = "留针时间长度不能超过50个字符")]
        public string RetentionTime { get; set; } = string.Empty;

        /// <summary>
        /// 刺激强度
        /// </summary>
        [StringLength(50, ErrorMessage = "刺激强度长度不能超过50个字符")]
        public string StimulationIntensity { get; set; } = string.Empty;

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
        /// 关联穴位
        /// </summary>
        [StringLength(200, ErrorMessage = "关联穴位长度不能超过200个字符")]
        public string AssociatedPoints { get; set; } = string.Empty;
    }
}