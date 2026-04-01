using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 推拿方案表（massage_plans）
    /// 记录某治疗方案（含版本）下的推拿操作细节
    /// </summary>
    [Table("massage_plans")]
    public class MassagePlan
    {
        /// <summary>
        /// 推拿方案唯一标识
        /// </summary>
        [Key]
        [Column("massage_id")]
        public long MassageId { get; set; }

        /// <summary>
        /// 关联治疗方案ID（外键）
        /// </summary>
        [Required]
        [Column("plan_id")]
        public long PlanId { get; set; }

        /// <summary>
        /// 推拿类型（枚举：TUINA=中医推拿，RELAX=放松推拿）
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("massage_type")]
        public string MassageType { get; set; } = null!;

        /// <summary>
        /// 操作部位（如“颈部：风池至大椎；肩部：肩井至肩髃”）
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Column("body_parts")]
        public string BodyParts { get; set; } = null!;

        /// <summary>
        /// 手法细节（如“滚法 5 分钟，按揉风池穴 1 分钟，力度中等”）
        /// </summary>
        [Required]
        [Column("techniques", TypeName = "text")]
        public string Techniques { get; set; } = null!;

        /// <summary>
        /// 单次时长（分钟）
        /// </summary>
        [Required]
        [Column("duration")]
        public int Duration { get; set; }

        /// <summary>
        /// 治疗频次（如“每周 3 次，共 2 周”）
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("frequency")]
        public string Frequency { get; set; } = null!;

        /// <summary>
        /// 执行推拿的医师ID（外键）
        /// </summary>
        [Required]
        [Column("operator_id")]
        public long OperatorId { get; set; }

        // public virtual User Operator { get; set; } = null!;
        // public virtual TreatmentPlan Plan { get; set; } = null!;
    }
}
