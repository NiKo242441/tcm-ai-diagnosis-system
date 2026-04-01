using System.ComponentModel.DataAnnotations;

namespace TcmAiDiagnosis.Entities.Enums
{
    /// <summary>
    /// 治疗方案全生命周期状态枚举（系统级强制标准）
    /// 覆盖计划1-7所有模块的状态流转需求
    /// </summary>
    public enum TreatmentStatus
    {
        /// <summary>
        /// 方案生成中 (计划2并发锁)
        /// </summary>
        [Display(Name = "方案生成中")]
        Generating = 0,
        /// <summary>
        /// AI已生成 (计划2输出)
        /// </summary>
        [Display(Name = "AI已生成")]
        AIGenerated = 1,
        /// <summary>
        /// 医生编辑中 (计划3)
        /// </summary>
        [Display(Name = "医生编辑中")]
        Editing = 2,
        /// <summary>
        /// 安全检查中 (计划4)
        /// </summary>
        [Display(Name = "安全检查中")]
        Checking = 3,
        /// <summary>
        /// 检查失败 (计划4输出)
        /// </summary>
        [Display(Name = "检查失败")]
        CheckFailed = 4,
        /// <summary>
        /// 版本管理中 (计划5)
        /// </summary>
        [Display(Name = "版本管理中")]
        Versioning = 5,
        /// <summary>
        /// 已定稿 (计划5输出)
        /// </summary>
        [Display(Name = "已定稿")]
        Finalized = 6,
        /// <summary>
        /// 已归档 (历史版本)
        /// </summary>
        [Display(Name = "已归档")]
        Archived = 7
    }
}