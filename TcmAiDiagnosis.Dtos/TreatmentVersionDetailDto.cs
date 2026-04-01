using System.ComponentModel.DataAnnotations;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 治疗方案版本详情数据传输对象
    /// </summary>
    public class TreatmentVersionDetailDto
    {
        /// <summary>
        /// 治疗方案ID
        /// </summary>
        public int TreatmentId { get; set; }

        /// <summary>
        /// 版本号，格式：V1.0.0
        /// </summary>
        [Required(ErrorMessage = "版本号不能为空")]
        [StringLength(20, ErrorMessage = "版本号长度不能超过20个字符")]
        public string Version { get; set; } = "V1.0.0";

        /// <summary>
        /// 是否最新版本
        /// </summary>
        public bool IsLatest { get; set; }

        /// <summary>
        /// 是否AI生成
        /// </summary>
        public bool IsAiOriginated { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public TreatmentStatus Status { get; set; }

        /// <summary>
        /// 诊断信息
        /// </summary>
        [StringLength(1000, ErrorMessage = "诊断信息长度不能超过1000个字符")]
        public string Diagnosis { get; set; } = string.Empty;

        /// <summary>
        /// 证候类型
        /// </summary>
        [StringLength(200, ErrorMessage = "证候类型长度不能超过200个字符")]
        public string SyndromeType { get; set; } = string.Empty;

        /// <summary>
        /// 治疗原则
        /// </summary>
        [StringLength(1000, ErrorMessage = "治疗原则长度不能超过1000个字符")]
        public string TreatmentPrinciple { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 创建者ID
        /// </summary>
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        public int? UpdatedByUserId { get; set; }

        /// <summary>
        /// 版本说明
        /// </summary>
        [StringLength(500, ErrorMessage = "版本说明长度不能超过500个字符")]
        public string VersionDescription { get; set; } = string.Empty;

        /// <summary>
        /// 变更内容摘要
        /// </summary>
        [StringLength(1000, ErrorMessage = "变更内容摘要长度不能超过1000个字符")]
        public string ChangeSummary { get; set; } = string.Empty;
    }
}