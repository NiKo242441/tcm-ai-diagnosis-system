using System.ComponentModel.DataAnnotations;

namespace TcmAiDiagnosis.Dtos
{
    public class CreateAlertRuleDto
    {
        [Required(ErrorMessage = "规则名称不能为空")]
        [StringLength(100, ErrorMessage = "规则名称长度不能超过100个字符")]
        public string RuleName { get; set; } = string.Empty;

        public int? HerbId { get; set; }

        [Required(ErrorMessage = "预警类型不能为空")]
        [StringLength(20, ErrorMessage = "预警类型长度不能超过20个字符")]
        public string AlertType { get; set; } = string.Empty;

        [Required(ErrorMessage = "阈值不能为空")]
        [Range(0.01, double.MaxValue, ErrorMessage = "阈值必须大于0")]
        public decimal Threshold { get; set; }

        [Required(ErrorMessage = "比较运算符不能为空")]
        [StringLength(10, ErrorMessage = "比较运算符长度不能超过10个字符")]
        public string ComparisonOperator { get; set; } = "LT";

        [StringLength(500, ErrorMessage = "通知人员ID长度不能超过500个字符")]
        public string? NotifyUserIds { get; set; }

        [Range(1, 3, ErrorMessage = "优先级必须在1-3之间")]
        public int Priority { get; set; } = 2;

        [StringLength(500, ErrorMessage = "备注长度不能超过500个字符")]
        public string? Remark { get; set; }

        [Required(ErrorMessage = "租户ID不能为空")]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "创建人不能为空")]
        public int CreatedBy { get; set; }
    }

    public class UpdateAlertRuleDto
    {
        [Required(ErrorMessage = "规则名称不能为空")]
        [StringLength(100, ErrorMessage = "规则名称长度不能超过100个字符")]
        public string RuleName { get; set; } = string.Empty;

        public int? HerbId { get; set; }

        [Required(ErrorMessage = "预警类型不能为空")]
        [StringLength(20, ErrorMessage = "预警类型长度不能超过20个字符")]
        public string AlertType { get; set; } = string.Empty;

        [Required(ErrorMessage = "阈值不能为空")]
        [Range(0.01, double.MaxValue, ErrorMessage = "阈值必须大于0")]
        public decimal Threshold { get; set; }

        [Required(ErrorMessage = "比较运算符不能为空")]
        [StringLength(10, ErrorMessage = "比较运算符长度不能超过10个字符")]
        public string ComparisonOperator { get; set; } = "LT";

        [StringLength(500, ErrorMessage = "通知人员ID长度不能超过500个字符")]
        public string? NotifyUserIds { get; set; }

        public bool IsEnabled { get; set; } = true;

        [Range(1, 3, ErrorMessage = "优先级必须在1-3之间")]
        public int Priority { get; set; } = 2;

        [StringLength(500, ErrorMessage = "备注长度不能超过500个字符")]
        public string? Remark { get; set; }

        [Required(ErrorMessage = "租户ID不能为空")]
        public int TenantId { get; set; }
    }

    public class ToggleAlertRuleDto
    {
        [Required(ErrorMessage = "启用状态不能为空")]
        public bool IsEnabled { get; set; }
    }

    public class BatchUpdateRuleStatusDto
    {
        [Required(ErrorMessage = "规则ID列表不能为空")]
        [MinLength(1, ErrorMessage = "至少选择一个规则")]
        public List<int> RuleIds { get; set; } = new();

        [Required(ErrorMessage = "启用状态不能为空")]
        public bool IsEnabled { get; set; }
    }

    public class AlertRuleQueryDto
    {
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "每页大小必须在1-100之间")]
        public int PageSize { get; set; } = 10;

        public string? SearchKeyword { get; set; }

        public int? TenantId { get; set; }

        public string? AlertType { get; set; }

        public bool? IsEnabled { get; set; }

        public int? HerbId { get; set; }
    }

    public class AlertRuleResponseDto
    {
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public int? HerbId { get; set; }
        public string? HerbName { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string AlertTypeName { get; set; } = string.Empty;
        public decimal Threshold { get; set; }
        public string ComparisonOperator { get; set; } = string.Empty;
        public string ComparisonOperatorName { get; set; } = string.Empty;
        public string? NotifyUserIds { get; set; }
        public List<string>? NotifyUserNames { get; set; }
        public bool IsEnabled { get; set; }
        public int Priority { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public int? TenantId { get; set; }
        public string? TenantName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public string? Remark { get; set; }
    }
}