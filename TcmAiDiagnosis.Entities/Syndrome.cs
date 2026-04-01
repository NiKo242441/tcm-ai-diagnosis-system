namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 证候诊断表
    /// </summary>
    public class Syndrome
    {
        /// <summary>
        /// 证候ID
        /// </summary>
        public int SyndromeId { get; set; }

        /// <summary>
        /// 就诊ID (外键)
        /// </summary>
        public int VisitId { get; set; }

        /// <summary>
        /// 证候名称
        /// </summary>
        public string SyndromeName { get; set; } = string.Empty;

        /// <summary>
        /// 置信度 (0-100)
        /// </summary>
        public decimal Confidence { get; set; }

        /// <summary>
        /// 主要症状 (JSON格式存储)
        /// </summary>
        public string MainSymptoms { get; set; } = string.Empty;

        /// <summary>
        /// 常见疾病 (JSON格式存储)
        /// </summary>
        public string CommonDiseases { get; set; } = string.Empty;

        /// <summary>
        /// 证候描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 病机分析
        /// </summary>
        public string? PathogenesisAnalysis { get; set; }

        /// <summary>
        /// 治疗原则
        /// </summary>
        public string? TreatmentPrinciple { get; set; }

        /// <summary>
        /// 推荐方剂 (JSON格式存储)
        /// </summary>
        public string? RecommendedFormulas { get; set; }

        /// <summary>
        /// 证候分类 (JSON格式存储，如：虚证、实证、寒证、热证等)
        /// </summary>
        public string? SyndromeCategories { get; set; }

        /// <summary>
        /// 归属脏腑 (JSON格式存储，如：心、肝、脾、肺、肾等)
        /// </summary>
        public string? RelatedOrgans { get; set; }

        /// <summary>
        /// 诊断信息 (JSON格式存储，包含诊断结论、分析、鉴别诊断)
        /// </summary>
        public string? DiagnosisInfo { get; set; }

        /// <summary>
        /// 治疗与护理信息 (JSON格式存储，包含治疗建议、注意事项)
        /// </summary>
        public string? TreatmentCareInfo { get; set; }

        /// <summary>
        /// 详情状态 (0-未获取, 1-获取中, 2-已获取, 3-获取失败)
        /// </summary>
        public int DetailStatus { get; set; } = 0;

        /// <summary>
        /// 是否确证 (只有一个证候可以被标记为确证)
        /// </summary>
        public bool IsConfirmed { get; set; } = false;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        // 导航属性
        public Visit Visit { get; set; }
    }
}