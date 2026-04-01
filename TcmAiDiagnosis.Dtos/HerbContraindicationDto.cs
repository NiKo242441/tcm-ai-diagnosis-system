using System.ComponentModel.DataAnnotations;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 中药配伍禁忌数据传输对象
    /// </summary>
    public class HerbContraindicationDto
    {
        /// <summary>
        /// 配伍禁忌ID
        /// </summary>
        public int ContraindicationId { get; set; }

        /// <summary>
        /// 主药材ID
        /// </summary>
        public int PrimaryHerbId { get; set; }

        /// <summary>
        /// 主药材名称
        /// </summary>
        public string PrimaryHerbName { get; set; } = string.Empty;

        /// <summary>
        /// 冲突药材ID
        /// </summary>
        public int ConflictHerbId { get; set; }

        /// <summary>
        /// 冲突药材名称
        /// </summary>
        public string ConflictHerbName { get; set; } = string.Empty;

        /// <summary>
        /// 配伍禁忌类型（如：十八反、十九畏、现代研究等）
        /// </summary>
        public string ContraindicationType { get; set; } = string.Empty;

        /// <summary>
        /// 风险等级（1-低风险，2-中风险，3-高风险，4-严重风险）
        /// </summary>
        public int RiskLevel { get; set; }

        /// <summary>
        /// 风险等级描述
        /// </summary>
        public string RiskLevelDescription { get; set; } = string.Empty;

        /// <summary>
        /// 禁忌描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 可能的不良反应
        /// </summary>
        public string? AdverseReactions { get; set; }

        /// <summary>
        /// 依据来源
        /// </summary>
        public string? Evidence { get; set; }

        /// <summary>
        /// 是否为绝对禁忌
        /// </summary>
        public bool IsAbsoluteContraindication { get; set; }

        /// <summary>
        /// 特殊说明
        /// </summary>
        public string? SpecialNotes { get; set; }

        /// <summary>
        /// 替代方案建议
        /// </summary>
        public string? AlternativeSuggestions { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 审核状态（0-待审核，1-已审核，2-审核不通过）
        /// </summary>
        public int ReviewStatus { get; set; }

        /// <summary>
        /// 审核状态描述
        /// </summary>
        public string ReviewStatusDescription { get; set; } = string.Empty;

        /// <summary>
        /// 审核意见
        /// </summary>
        public string? ReviewComments { get; set; }

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
    /// 创建配伍禁忌请求DTO
    /// </summary>
    public class CreateHerbContraindicationDto
    {
        /// <summary>
        /// 主药材ID
        /// </summary>
        [Required(ErrorMessage = "主药材ID不能为空")]
        public int PrimaryHerbId { get; set; }

        /// <summary>
        /// 冲突药材ID
        /// </summary>
        [Required(ErrorMessage = "冲突药材ID不能为空")]
        public int ConflictHerbId { get; set; }

        /// <summary>
        /// 配伍禁忌类型
        /// </summary>
        [Required(ErrorMessage = "配伍禁忌类型不能为空")]
        [StringLength(50, ErrorMessage = "配伍禁忌类型长度不能超过50个字符")]
        public string ContraindicationType { get; set; } = string.Empty;

        /// <summary>
        /// 风险等级（1-低风险，2-中风险，3-高风险，4-严重风险）
        /// </summary>
        [Required(ErrorMessage = "风险等级不能为空")]
        [Range(1, 4, ErrorMessage = "风险等级必须在1-4之间")]
        public int RiskLevel { get; set; } = 1;

        /// <summary>
        /// 禁忌描述
        /// </summary>
        [Required(ErrorMessage = "禁忌描述不能为空")]
        [StringLength(500, ErrorMessage = "禁忌描述长度不能超过500个字符")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 可能的不良反应
        /// </summary>
        [StringLength(1000, ErrorMessage = "不良反应描述长度不能超过1000个字符")]
        public string? AdverseReactions { get; set; }

        /// <summary>
        /// 依据来源
        /// </summary>
        [StringLength(200, ErrorMessage = "依据来源长度不能超过200个字符")]
        public string? Evidence { get; set; }

        /// <summary>
        /// 是否为绝对禁忌
        /// </summary>
        public bool IsAbsoluteContraindication { get; set; } = true;

        /// <summary>
        /// 特殊说明
        /// </summary>
        [StringLength(1000, ErrorMessage = "特殊说明长度不能超过1000个字符")]
        public string? SpecialNotes { get; set; }

        /// <summary>
        /// 替代方案建议
        /// </summary>
        [StringLength(500, ErrorMessage = "替代方案建议长度不能超过500个字符")]
        public string? AlternativeSuggestions { get; set; }
    }

    /// <summary>
    /// 更新配伍禁忌请求DTO
    /// </summary>
    public class UpdateHerbContraindicationDto : CreateHerbContraindicationDto
    {
        /// <summary>
        /// 配伍禁忌ID
        /// </summary>
        [Required(ErrorMessage = "配伍禁忌ID不能为空")]
        public int ContraindicationId { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 配伍禁忌查询条件DTO
    /// </summary>
    public class HerbContraindicationQueryDto
    {
        /// <summary>
        /// 主药材ID
        /// </summary>
        public int? PrimaryHerbId { get; set; }

        /// <summary>
        /// 冲突药材ID
        /// </summary>
        public int? ConflictHerbId { get; set; }

        /// <summary>
        /// 药材名称（模糊查询，可以是主药材或冲突药材）
        /// </summary>
        public string? HerbName { get; set; }

        /// <summary>
        /// 配伍禁忌类型
        /// </summary>
        public string? ContraindicationType { get; set; }

        /// <summary>
        /// 风险等级
        /// </summary>
        public int? RiskLevel { get; set; }

        /// <summary>
        /// 是否为绝对禁忌
        /// </summary>
        public bool? IsAbsoluteContraindication { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public int? ReviewStatus { get; set; }

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

    /// <summary>
    /// 配伍禁忌检查请求DTO
    /// </summary>
    public class ContraindicationCheckRequest
    {
        /// <summary>
        /// 需要检查的药材ID列表
        /// </summary>
        [Required(ErrorMessage = "药材ID列表不能为空")]
        [MinLength(2, ErrorMessage = "至少需要2个药材进行配伍禁忌检查")]
        public List<int> HerbIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// 配伍禁忌检查结果DTO
    /// </summary>
    public class ContraindicationCheckResult
    {
        /// <summary>
        /// 是否存在配伍禁忌
        /// </summary>
        public bool HasContraindications { get; set; }

        /// <summary>
        /// 检查到的配伍禁忌列表
        /// </summary>
        public List<HerbContraindicationDto> Contraindications { get; set; } = new List<HerbContraindicationDto>();

        /// <summary>
        /// 最高风险等级
        /// </summary>
        public int MaxRiskLevel { get; set; }

        /// <summary>
        /// 风险评估摘要
        /// </summary>
        public string RiskSummary { get; set; } = string.Empty;

        /// <summary>
        /// 建议措施
        /// </summary>
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    /// <summary>
    /// 配伍禁忌审核请求DTO
    /// </summary>
    public class ReviewHerbContraindicationDto
    {
        /// <summary>
        /// 配伍禁忌ID
        /// </summary>
        [Required(ErrorMessage = "配伍禁忌ID不能为空")]
        public int ContraindicationId { get; set; }

        /// <summary>
        /// 审核状态（1-已审核，2-审核不通过）
        /// </summary>
        [Required(ErrorMessage = "审核状态不能为空")]
        [Range(1, 2, ErrorMessage = "审核状态必须为1（已审核）或2（审核不通过）")]
        public int ReviewStatus { get; set; }

        /// <summary>
        /// 审核意见
        /// </summary>
        [StringLength(500, ErrorMessage = "审核意见长度不能超过500个字符")]
        public string? ReviewComments { get; set; }
    }
}