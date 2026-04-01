using System.ComponentModel.DataAnnotations;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 治疗方案变更详情数据传输对象
    /// </summary>
    public class TreatmentChangeDetailDto
    {
        /// <summary>
        /// 变更记录ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 治疗方案ID
        /// </summary>
        public int TreatmentId { get; set; }

        /// <summary>
        /// 变更前版本
        /// </summary>
        [StringLength(20, ErrorMessage = "版本号长度不能超过20个字符")]
        public string PreviousVersion { get; set; } = string.Empty;

        /// <summary>
        /// 变更后版本
        /// </summary>
        [StringLength(20, ErrorMessage = "版本号长度不能超过20个字符")]
        public string NewVersion { get; set; } = string.Empty;

        /// <summary>
        /// 变更类型
        /// </summary>
        [Required(ErrorMessage = "变更类型不能为空")]
        [StringLength(100, ErrorMessage = "变更类型长度不能超过100个字符")]
        public string ChangeType { get; set; } = string.Empty;

        /// <summary>
        /// 变更字段
        /// </summary>
        [Required(ErrorMessage = "变更字段不能为空")]
        [StringLength(200, ErrorMessage = "变更字段长度不能超过200个字符")]
        public string ChangedField { get; set; } = string.Empty;

        /// <summary>
        /// 变更前值
        /// </summary>
        [StringLength(2000, ErrorMessage = "变更前值长度不能超过2000个字符")]
        public string PreviousValue { get; set; } = string.Empty;

        /// <summary>
        /// 变更后值
        /// </summary>
        [StringLength(2000, ErrorMessage = "变更后值长度不能超过2000个字符")]
        public string NewValue { get; set; } = string.Empty;

        /// <summary>
        /// 变更原因
        /// </summary>
        [StringLength(1000, ErrorMessage = "变更原因长度不能超过1000个字符")]
        public string ChangeReason { get; set; } = string.Empty;

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; set; }

        /// <summary>
        /// 变更人ID
        /// </summary>
        public int ChangedByUserId { get; set; }

        /// <summary>
        /// 变更人姓名
        /// </summary>
        [StringLength(100, ErrorMessage = "变更人姓名长度不能超过100个字符")]
        public string ChangedByUsername { get; set; } = string.Empty;

        /// <summary>
        /// 租户ID
        /// </summary>
        public int TenantId { get; set; }
    }
}