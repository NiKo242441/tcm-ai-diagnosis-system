using System.ComponentModel.DataAnnotations;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 治疗方案DTO - 通用的、功能完备的DTO，用于处理治疗方案的展示、创建和编辑
    /// </summary>
    public class TreatmentDto
    {
        // --- 核心标识与元数据 ---
        /// <summary>
        /// 治疗方案ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 患者ID
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// 就诊ID
        /// </summary>
        public int VisitId { get; set; }

        /// <summary>
        /// 确证证候ID
        /// </summary>
        public int SyndromeId { get; set; }

        /// <summary>
        /// 版本号 (例如: "V1.0.1")
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 方案状态
        /// </summary>
        public TreatmentStatus Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 创建医生姓名
        /// </summary>
        public string CreatorName { get; set; } = string.Empty;

        /// <summary>
        /// 是否为AI生成的原始版本
        /// </summary>
        public bool IsAiOriginated { get; set; }

        /// <summary>
        /// 是否为最新版本
        /// </summary>
        public bool IsLatest { get; set; }

        // --- 顶层诊断与原则信息 ---
        /// <summary>
        /// 中医诊断
        /// </summary>
        public string TcmDiagnosis { get; set; } = string.Empty;

        /// <summary>
        /// 证候分析
        /// </summary>
        public string SyndromeAnalysis { get; set; } = string.Empty;

        /// <summary>
        /// 治疗原则
        /// </summary>
        public string TreatmentPrinciple { get; set; } = string.Empty;

        /// <summary>
        /// 预期效果
        /// </summary>
        public string ExpectedOutcome { get; set; } = string.Empty;

        /// <summary>
        /// 总体注意事项
        /// </summary>
        public string Precautions { get; set; } = string.Empty;

        // --- 聚合的上下文信息 (用于展示) ---
        /// <summary>
        /// 患者基本信息
        /// </summary>
        public PatientBriefDto? PatientInfo { get; set; }

        /// <summary>
        /// 证候基本信息
        /// </summary>
        public SyndromeBriefDto? SyndromeInfo { get; set; }

        // --- 九大治疗模块内容 ---
        /// <summary>
        /// 中药处方列表
        /// </summary>
        public List<PrescriptionDto> Prescriptions { get; set; } = new();

        /// <summary>
        /// 针刺治疗列表
        /// </summary>
        public List<AcupunctureDto> Acupunctures { get; set; } = new();

        /// <summary>
        /// 艾灸治疗列表
        /// </summary>
        public List<MoxibustionDto> Moxibustions { get; set; } = new();

        /// <summary>
        /// 拔罐治疗列表
        /// </summary>
        public List<CuppingDto> Cuppings { get; set; } = new();

        /// <summary>
        /// 食疗方案列表
        /// </summary>
        public List<DietaryTherapyDto> DietaryTherapies { get; set; } = new();

        /// <summary>
        /// 生活方式建议列表
        /// </summary>
        public List<LifestyleAdviceDto> LifestyleAdvices { get; set; } = new();

        /// <summary>
        /// 饮食建议列表
        /// </summary>
        public List<DietaryAdviceDto> DietaryAdvices { get; set; } = new();

        /// <summary>
        /// 随访建议列表
        /// </summary>
        public List<FollowUpAdviceDto> FollowUpAdvices { get; set; } = new();

        // --- 安全与警告信息 ---
        /// <summary>
        /// 中药安全警告列表
        /// </summary>
        public List<HerbalWarningDto> HerbalWarnings { get; set; } = new();

        /// <summary>
        /// 食疗安全警告列表
        /// </summary>
        public List<DietaryWarningDto> DietaryWarnings { get; set; } = new();
    }

    /// <summary>
    /// 治疗方案列表项DTO - 用于在列表中展示治疗方案的摘要信息
    /// </summary>
    public class TreatmentListItemDto
    {
        /// <summary>
        /// 治疗方案ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 患者ID
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// 患者姓名
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        /// <summary>
        /// 中医诊断
        /// </summary>
        public string TcmDiagnosis { get; set; } = string.Empty;

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 方案状态
        /// </summary>
        public TreatmentStatus Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 创建医生姓名
        /// </summary>
        public string CreatorName { get; set; } = string.Empty;
    }
}