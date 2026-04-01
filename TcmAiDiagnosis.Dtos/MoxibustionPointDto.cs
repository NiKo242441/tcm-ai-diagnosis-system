using System.ComponentModel.DataAnnotations;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 艾灸点位基础数据传输对象
    /// </summary>
    public class MoxibustionPointDto
    {
        /// <summary>
        /// 艾灸点位ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联艾灸ID
        /// </summary>
        public int MoxibustionId { get; set; }

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
        /// 艾灸方法
        /// </summary>
        [StringLength(200, ErrorMessage = "艾灸方法长度不能超过200个字符")]
        public string MoxibustionMethod { get; set; } = string.Empty;

        /// <summary>
        /// 艾灸时间
        /// </summary>
        [StringLength(50, ErrorMessage = "艾灸时间长度不能超过50个字符")]
        public string MoxibustionTime { get; set; } = string.Empty;

        /// <summary>
        /// 艾灸温度
        /// </summary>
        [StringLength(50, ErrorMessage = "艾灸温度长度不能超过50个字符")]
        public string MoxibustionTemperature { get; set; } = string.Empty;

        /// <summary>
        /// 艾炷/艾条大小
        /// </summary>
        [StringLength(50, ErrorMessage = "艾炷/艾条大小长度不能超过50个字符")]
        public string MoxaSize { get; set; } = string.Empty;

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
    /// 艾灸点位详情数据传输对象
    /// </summary>
    public class MoxibustionPointDetailDto : MoxibustionPointDto
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
    /// 创建艾灸点位请求DTO
    /// </summary>
    public class MoxibustionPointCreateDto
    {
        /// <summary>
        /// 关联艾灸ID
        /// </summary>
        [Required(ErrorMessage = "艾灸ID不能为空")]
        public int MoxibustionId { get; set; }

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
        /// 艾灸方法
        /// </summary>
        [StringLength(200, ErrorMessage = "艾灸方法长度不能超过200个字符")]
        public string MoxibustionMethod { get; set; } = string.Empty;

        /// <summary>
        /// 艾灸时间
        /// </summary>
        [StringLength(50, ErrorMessage = "艾灸时间长度不能超过50个字符")]
        public string MoxibustionTime { get; set; } = string.Empty;

        /// <summary>
        /// 艾灸温度
        /// </summary>
        [StringLength(50, ErrorMessage = "艾灸温度长度不能超过50个字符")]
        public string MoxibustionTemperature { get; set; } = string.Empty;

        /// <summary>
        /// 艾炷/艾条大小
        /// </summary>
        [StringLength(50, ErrorMessage = "艾炷/艾条大小长度不能超过50个字符")]
        public string MoxaSize { get; set; } = string.Empty;

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
    /// 更新艾灸点位请求DTO
    /// </summary>
    public class MoxibustionPointUpdateDto
    {
        /// <summary>
        /// 艾灸点位ID
        /// </summary>
        [Required(ErrorMessage = "艾灸点位ID不能为空")]
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
        /// 艾灸方法
        /// </summary>
        [StringLength(200, ErrorMessage = "艾灸方法长度不能超过200个字符")]
        public string MoxibustionMethod { get; set; } = string.Empty;

        /// <summary>
        /// 艾灸时间
        /// </summary>
        [StringLength(50, ErrorMessage = "艾灸时间长度不能超过50个字符")]
        public string MoxibustionTime { get; set; } = string.Empty;

        /// <summary>
        /// 艾灸温度
        /// </summary>
        [StringLength(50, ErrorMessage = "艾灸温度长度不能超过50个字符")]
        public string MoxibustionTemperature { get; set; } = string.Empty;

        /// <summary>
        /// 艾炷/艾条大小
        /// </summary>
        [StringLength(50, ErrorMessage = "艾炷/艾条大小长度不能超过50个字符")]
        public string MoxaSize { get; set; } = string.Empty;

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