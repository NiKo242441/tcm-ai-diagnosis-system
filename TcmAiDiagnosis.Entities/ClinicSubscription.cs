using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 诊所订阅表（clinic_subscriptions）
    /// </summary>
    [Table("clinic_subscriptions")]
    public class ClinicSubscription
    {
        /// <summary>
        /// 订阅ID
        /// </summary>
        [Key]
        [Column("subscription_id")]
        public long SubscriptionId { get; set; }

        /// <summary>
        /// 诊所ID（外键）
        /// </summary>
        [Required]
        [Column("clinic_id")]
        public long ClinicId { get; set; }

        /// <summary>
        /// 计费方案ID（外键）
        /// </summary>
        [Required]
        [Column("billing_plan_id")]
        public long BillingPlanId { get; set; }

        /// <summary>
        /// 生效时间
        /// </summary>
        [Required]
        [Column("effective_time")]
        public DateTime EffectiveTime { get; set; }

        /// <summary>
        /// 失效时间
        /// </summary>
        [Required]
        [Column("expiration_time")]
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = null!;
    }
}