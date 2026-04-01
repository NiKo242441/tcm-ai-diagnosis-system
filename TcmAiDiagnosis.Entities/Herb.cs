using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Entities
{
    public class Herb
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 药材名称 (对应DifyAPI的name字段)
        /// </summary>
        
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 药材别名
        /// </summary>
        public string Aliases { get; set; } = string.Empty;
        
        /// <summary>
        /// 药性 (如：甘、平；苦、寒等)
        /// </summary>
        
        public string Properties { get; set; } = string.Empty;
        
        /// <summary>
        /// 归经 (如：心、肺、脾、胃经)
        /// </summary>
        
        public string Meridians { get; set; } = string.Empty;
        
        /// <summary>
        /// 功效主治
        /// </summary>
        
        public string Efficacy { get; set; } = string.Empty;
        
        /// <summary>
        /// 药材分类 (如：补益药、泻下药、化痰药等)
        /// </summary>
        
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// 毒性等级 (无毒、小毒、有毒、大毒)
        /// </summary>
        public string ToxicityLevel { get; set; } = string.Empty;
        
        /// <summary>
        /// 常用剂量范围
        /// </summary>
        public string DosageRange { get; set; } = string.Empty;
        
        /// <summary>
        /// 常用单位
        /// </summary>
        public string CommonUnit { get; set; } = "g";
        
        /// <summary>
        /// 使用注意事项
        /// </summary>
        public string Precautions { get; set; } = string.Empty;
        
        /// <summary>
        /// 禁忌症
        /// </summary>
        public string Contraindications { get; set; } = string.Empty;
        
        /// <summary>
        /// 炮制方法
        /// </summary>
        public string ProcessingMethods { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否为常用药材
        /// </summary>
        public bool IsCommonlyUsed { get; set; } = false;
        
        /// <summary>
        /// 是否需要特殊处理 (如先煎、后下、包煎等)
        /// </summary>
        public bool RequiresSpecialHandling { get; set; } = false;
        
        /// <summary>
        /// 特殊用法说明
        /// </summary>
        public string SpecialUsageInstructions { get; set; } = string.Empty;
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// 创建用户ID
        /// </summary>
        public int? CreatedByUserId { get; set; }
        
        /// <summary>
        /// 更新用户ID
        /// </summary>
        public int? UpdatedByUserId { get; set; }
        
        /// <summary>
        /// 软删除标志
        /// </summary>
        public bool IsDeleted { get; set; } = false;
        
        /// <summary>
        /// 租户ID
        /// </summary>
        public int TenantId { get; set; }
        
        /// <summary>
        /// 是否AI生成 (统一命名规范)
        /// </summary>
        public bool IsAiOriginated { get; set; } = false;
        
        /// <summary>
        /// 状态 (草稿/已确认/已归档)
        /// </summary>
        public ReviewStatus Status { get; set; } = ReviewStatus.AIGenerated;
        
        // 导航属性
        
        /// <summary>
        /// 配伍禁忌记录列表
        /// </summary>
        public virtual ICollection<HerbContraindication> HerbContraindications { get; set; } = new List<HerbContraindication>();
    }
}