using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 食疗建议表（dietary_suggestions）
    /// 记录某治疗方案（含版本）下的饮食调理建议
    /// </summary>
    [Table("dietary_suggestions")]
    public class DietarySuggestion
    {
        /// <summary>
        /// 食疗建议唯一标识
        /// </summary>
        [Key]
        [Column("dietary_id")]
        public long DietaryId { get; set; }

        /// <summary>
        /// 关联治疗方案ID（外键）
        /// </summary>
        [Required]
        [Column("plan_id")]
        public long PlanId { get; set; }

        /// <summary>
        /// 食疗原则（如“风热证，宜清热生津”）
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Column("principle")]
        public string Principle { get; set; } = null!;

        /// <summary>
        /// 推荐食物及食谱（如“梨、冬瓜、金银花茶…”）
        /// </summary>
        [Required]
        [Column("recommended_food", TypeName = "text")]
        public string RecommendedFood { get; set; } = null!;

        /// <summary>
        /// 禁忌食物（如“羊肉、辣椒、油炸食品”）
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Column("forbidden_food")]
        public string ForbiddenFood { get; set; } = null!;

        /// <summary>
        /// 适用周期（如“发病期间至痊愈后3天”）
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("valid_period")]
        public string ValidPeriod { get; set; } = null!;
    }
}
