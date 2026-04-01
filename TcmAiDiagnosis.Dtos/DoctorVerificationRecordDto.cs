using System;
using System.ComponentModel.DataAnnotations;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 医生验证记录DTO
    /// </summary>
    public class DoctorVerificationRecordDto
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 医生ID
        /// </summary>
        public int DoctorId { get; set; }
        
        /// <summary>
        /// 关联治疗方案ID
        /// </summary>
        public int TreatmentId { get; set; }
        
        /// <summary>
        /// 操作类型
        /// </summary>
        public string OperationType { get; set; } = string.Empty;
        
        /// <summary>
        /// 风险级别
        /// </summary>
        public string RiskLevel { get; set; } = string.Empty;
        
        /// <summary>
        /// 验证方法
        /// </summary>
        public string VerificationMethod { get; set; } = string.Empty;
        
        /// <summary>
        /// 验证结果
        /// </summary>
        public bool VerificationResult { get; set; }
        
        /// <summary>
        /// 验证失败原因或操作说明
        /// </summary>
        public string Reason { get; set; } = string.Empty;
        
        /// <summary>
        /// 客户端IP地址
        /// </summary>
        public string ClientIp { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户代理信息
        /// </summary>
        public string UserAgent { get; set; } = string.Empty;
        
        /// <summary>
        /// 验证时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 验证有效期
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        /// <summary>
        /// 是否仍然有效
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// 租户ID
        /// </summary>
        public int TenantId { get; set; }
    }
    
    /// <summary>
    /// 医生验证记录创建DTO
    /// </summary>
    public class DoctorVerificationRecordCreateDto
    {
        /// <summary>
        /// 医生ID
        /// </summary>
        [Required]
        public int DoctorId { get; set; }
        
        /// <summary>
        /// 关联治疗方案ID
        /// </summary>
        [Required]
        public int TreatmentId { get; set; }
        
        /// <summary>
        /// 操作类型
        /// </summary>
        [Required]
        [StringLength(100)]
        public string OperationType { get; set; } = string.Empty;
        
        /// <summary>
        /// 风险级别
        /// </summary>
        [Required]
        [StringLength(20)]
        public string RiskLevel { get; set; } = string.Empty;
        
        /// <summary>
        /// 验证方法
        /// </summary>
        [Required]
        [StringLength(50)]
        public string VerificationMethod { get; set; } = string.Empty;
        
        /// <summary>
        /// 验证结果
        /// </summary>
        [Required]
        public bool VerificationResult { get; set; }
        
        /// <summary>
        /// 验证失败原因或操作说明
        /// </summary>
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
        
        /// <summary>
        /// 客户端IP地址
        /// </summary>
        [Required]
        [StringLength(45)]
        public string ClientIp { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户代理信息
        /// </summary>
        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty;
        
        /// <summary>
        /// 验证有效期
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        /// <summary>
        /// 租户ID
        /// </summary>
        [Required]
        public int TenantId { get; set; }
    }
    
    /// <summary>
    /// 医生验证记录更新DTO
    /// </summary>
    public class DoctorVerificationRecordUpdateDto
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Required]
        public int Id { get; set; }
        
        /// <summary>
        /// 是否仍然有效
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// 验证有效期
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        /// <summary>
        /// 更新原因
        /// </summary>
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}