using System.ComponentModel.DataAnnotations;

namespace TcmAiDiagnosis.Entities.Enums
{
    /// <summary>
    /// 审核状态枚举
    /// </summary>
    public enum ReviewStatus
    {
        /// <summary>
        /// 待审核
        /// </summary>
        [Display(Name = "待审核")]
        Pending = 1,

        /// <summary>
        /// 审核通过
        /// </summary>
        [Display(Name = "审核通过")]
        Approved = 2,

        /// <summary>
        /// 审核拒绝
        /// </summary>
        [Display(Name = "审核拒绝")]
        Rejected = 3,

        /// <summary>
        /// 已取消
        /// </summary>
        [Display(Name = "已取消")]
        Cancelled = 4,
        
        /// <summary>
        /// AI已生成
        /// </summary>
        [Display(Name = "AI已生成")]
        AIGenerated = 5,
    }
}