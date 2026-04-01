using System.ComponentModel.DataAnnotations;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 中药材数据传输对象
    /// </summary>
    public class HerbDto
    {
        /// <summary>
        /// 药材ID
        /// </summary>
        public int HerbId { get; set; }

        /// <summary>
        /// 药材名称
        /// </summary>
        [Required(ErrorMessage = "药材名称不能为空")]
        [StringLength(100, ErrorMessage = "药材名称长度不能超过100个字符")]
        public string HerbName { get; set; } = string.Empty;

        /// <summary>
        /// 性味（如：苦、寒等）
        /// </summary>
        [StringLength(200, ErrorMessage = "性味描述长度不能超过200个字符")]
        public string? Properties { get; set; }

        /// <summary>
        /// 归经（如：心、肝、脾等）
        /// </summary>
        [StringLength(200, ErrorMessage = "归经描述长度不能超过200个字符")]
        public string? Meridians { get; set; }

        /// <summary>
        /// 功效
        /// </summary>
        [StringLength(500, ErrorMessage = "功效描述长度不能超过500个字符")]
        public string? Efficacy { get; set; }

        /// <summary>
        /// 类别（如：清热药、补益药等）
        /// </summary>
        [StringLength(100, ErrorMessage = "类别描述长度不能超过100个字符")]
        public string? Category { get; set; }

        /// <summary>
        /// 用法用量
        /// </summary>
        [StringLength(100, ErrorMessage = "用法用量描述长度不能超过100个字符")]
        public string? Dosage { get; set; }

        /// <summary>
        /// 注意事项
        /// </summary>
        [StringLength(1000, ErrorMessage = "注意事项描述长度不能超过1000个字符")]
        public string? Precautions { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        [StringLength(200, ErrorMessage = "来源描述长度不能超过200个字符")]
        public string? Source { get; set; }

        /// <summary>
        /// 炮制方法
        /// </summary>
        [StringLength(500, ErrorMessage = "炮制方法描述长度不能超过500个字符")]
        public string? ProcessingMethod { get; set; }

        /// <summary>
        /// 毒性等级（如：无毒、小毒、有毒、大毒）
        /// </summary>
        [StringLength(50, ErrorMessage = "毒性等级描述长度不能超过50个字符")]
        public string? ToxicityLevel { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(1000, ErrorMessage = "备注长度不能超过1000个字符")]
        public string? Remarks { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 创建药材请求DTO
    /// </summary>
    public class CreateHerbDto
    {
        /// <summary>
        /// 药材名称
        /// </summary>
        [Required(ErrorMessage = "药材名称不能为空")]
        [StringLength(100, ErrorMessage = "药材名称长度不能超过100个字符")]
        public string HerbName { get; set; } = string.Empty;

        /// <summary>
        /// 性味（如：苦、寒等）
        /// </summary>
        [StringLength(200, ErrorMessage = "性味描述长度不能超过200个字符")]
        public string? Properties { get; set; }

        /// <summary>
        /// 归经（如：心、肝、脾等）
        /// </summary>
        [StringLength(200, ErrorMessage = "归经描述长度不能超过200个字符")]
        public string? Meridians { get; set; }

        /// <summary>
        /// 功效
        /// </summary>
        [StringLength(500, ErrorMessage = "功效描述长度不能超过500个字符")]
        public string? Efficacy { get; set; }

        /// <summary>
        /// 类别（如：清热药、补益药等）
        /// </summary>
        [StringLength(100, ErrorMessage = "类别描述长度不能超过100个字符")]
        public string? Category { get; set; }

        /// <summary>
        /// 用法用量
        /// </summary>
        [StringLength(100, ErrorMessage = "用法用量描述长度不能超过100个字符")]
        public string? Dosage { get; set; }

        /// <summary>
        /// 注意事项
        /// </summary>
        [StringLength(1000, ErrorMessage = "注意事项描述长度不能超过1000个字符")]
        public string? Precautions { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        [StringLength(200, ErrorMessage = "来源描述长度不能超过200个字符")]
        public string? Source { get; set; }

        /// <summary>
        /// 炮制方法
        /// </summary>
        [StringLength(500, ErrorMessage = "炮制方法描述长度不能超过500个字符")]
        public string? ProcessingMethod { get; set; }

        /// <summary>
        /// 毒性等级（如：无毒、小毒、有毒、大毒）
        /// </summary>
        [StringLength(50, ErrorMessage = "毒性等级描述长度不能超过50个字符")]
        public string? ToxicityLevel { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(1000, ErrorMessage = "备注长度不能超过1000个字符")]
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// 更新药材请求DTO
    /// </summary>
    public class UpdateHerbDto : CreateHerbDto
    {
        /// <summary>
        /// 药材ID
        /// </summary>
        [Required(ErrorMessage = "药材ID不能为空")]
        public int HerbId { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 药材查询条件DTO
    /// </summary>
    public class HerbQueryDto
    {
        /// <summary>
        /// 药材名称（模糊查询）
        /// </summary>
        public string? HerbName { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 毒性等级
        /// </summary>
        public string? ToxicityLevel { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        [Range(1, 100, ErrorMessage = "每页大小必须在1-100之间")]
        public int PageSize { get; set; } = 20;
    }
}