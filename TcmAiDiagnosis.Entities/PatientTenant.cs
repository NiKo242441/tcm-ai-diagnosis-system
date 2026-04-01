using System;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 患者-租户关联表（多对多中间表）
    /// </summary>
    public class PatientTenant
    {
        public int PatientUserId { get; set; }
        public int TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// 添加该关联的医生ID
        /// </summary>
        public int? AddedBy { get; set; }
        
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime? AddedAt { get; set; }
        
        /// <summary>
        /// 关联是否激活
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// 关联类型：1-原始注册，2-跨租户添加
        /// </summary>
        public PatientTenantRelationType? RelationType { get; set; } = PatientTenantRelationType.Original;
        
        /// <summary>
        /// 重新激活该关联的医生ID
        /// </summary>
        public int? ReactivatedBy { get; set; }
        
        /// <summary>
        /// 重新激活时间
        /// </summary>
        public DateTime? ReactivatedAt { get; set; }
        
        /// <summary>
        /// 移除该关联的医生ID
        /// </summary>
        public int? RemovedBy { get; set; }
        
        /// <summary>
        /// 移除时间
        /// </summary>
        public DateTime? RemovedAt { get; set; }

        // 导航属性
        public User PatientUser { get; set; }
        public Tenant Tenant { get; set; }
    }
    
    /// <summary>
    /// 患者-租户关联类型
    /// </summary>
    public enum PatientTenantRelationType
    {
        /// <summary>
        /// 原始注册
        /// </summary>
        Original = 1,
        
        /// <summary>
        /// 跨租户添加
        /// </summary>
        CrossTenant = 2
    }
}