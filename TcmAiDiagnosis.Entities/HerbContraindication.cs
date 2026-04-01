using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 中药配伍禁忌表
    /// </summary>
    [Table("HerbContraindications")]
    public class HerbContraindication
    {
        /// <summary>
        /// 配伍禁忌ID（主键）
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ContraindicationId { get; set; }

        /// <summary>
        /// 主药材ID（外键）
        /// </summary>
        
        public int PrimaryHerbId { get; set; }

        /// <summary>
        /// 冲突药材ID（外键）
        /// </summary>
        
        public int ConflictHerbId { get; set; }

        /// <summary>
        /// 配伍禁忌类型（如：十八反、十九畏、现代研究等）
        /// </summary>
        
        public string ContraindicationType { get; set; } = string.Empty;

        /// <summary>
        /// 风险等级（1-低风险，2-中风险，3-高风险，4-严重风险）
        /// </summary>
        [Range(1, 4)]
        public int RiskLevel { get; set; } = 1;

        /// <summary>
        /// 禁忌描述
        /// </summary>
        
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 可能的不良反应
        /// </summary>
        public string? AdverseReactions { get; set; }

        /// <summary>
        /// 依据来源（如：《神农本草经》、现代药理研究等）
        /// </summary>
        public string? Evidence { get; set; }

        /// <summary>
        /// 是否为绝对禁忌（true-绝对禁忌，false-相对禁忌）
        /// </summary>
        public bool IsAbsoluteContraindication { get; set; } = true;

        /// <summary>
        /// 特殊说明（如特定条件下可以使用的情况）
        /// </summary>
        public string? SpecialNotes { get; set; }

        /// <summary>
        /// 替代方案建议
        /// </summary>
        public string? AlternativeSuggestions { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 创建者ID
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// 最后更新者ID
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// 审核状态（0-待审核，1-已审核，2-审核不通过）
        /// </summary>
        public int ReviewStatus { get; set; } = 0;

        /// <summary>
        /// 审核者ID
        /// </summary>
        public int? ReviewedBy { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// 审核意见
        /// </summary>
        public string? ReviewComments { get; set; }

        // 导航属性
        /// <summary>
        /// 主药材
        /// </summary>
        [ForeignKey(nameof(PrimaryHerbId))]
        public virtual Herb PrimaryHerb { get; set; } = null!;

        /// <summary>
        /// 冲突药材
        /// </summary>
        [ForeignKey(nameof(ConflictHerbId))]
        public virtual Herb ConflictHerb { get; set; } = null!;

        /// <summary>
        /// 创建者
        /// </summary>
        [ForeignKey(nameof(CreatedBy))]
        public virtual User? Creator { get; set; }

        /// <summary>
        /// 最后更新者
        /// </summary>
        [ForeignKey(nameof(UpdatedBy))]
        public virtual User? Updater { get; set; }

        /// <summary>
        /// 审核者
        /// </summary>
        [ForeignKey(nameof(ReviewedBy))]
        public virtual User? Reviewer { get; set; }
    }
}