using System.Text.Json.Serialization;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 证候概览数据传输对象
    /// </summary>
    public class SyndromeOverviewDto
    {
        public string SyndromeName { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
        public List<string> MainSymptoms { get; set; } = new List<string>();
        public List<string> CommonDiseases { get; set; } = new List<string>();
    }

    /// <summary>
    /// 证候详情数据传输对象
    /// </summary>
    public class SyndromeDetailDto
    {
        public string SyndromeName { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PathogenesisAnalysis { get; set; } = string.Empty;
        public string TreatmentPrinciple { get; set; } = string.Empty;
        public List<string> RecommendedFormulas { get; set; } = new List<string>();
        public List<string> MainSymptoms { get; set; } = new List<string>();
        public List<string> CommonDiseases { get; set; } = new List<string>();
        
        /// <summary>
        /// 证候分类（如：虚证、实证、寒证、热证等）
        /// </summary>
        public List<string> SyndromeCategories { get; set; } = new List<string>();
        
        /// <summary>
        /// 归属脏腑（如：心、肝、脾、肺、肾等）
        /// </summary>
        public List<string> RelatedOrgans { get; set; } = new List<string>();
        
        /// <summary>
        /// 诊断信息
        /// </summary>
        public DiagnosisInfo DiagnosisInfo { get; set; } = new DiagnosisInfo();
        
        /// <summary>
        /// 治疗与护理信息
        /// </summary>
        public TreatmentCareInfo TreatmentCareInfo { get; set; } = new TreatmentCareInfo();
    }

    /// <summary>
    /// 诊断信息
    /// </summary>
    public class DiagnosisInfo
    {
        /// <summary>
        /// 诊断结论
        /// </summary>
        public DiagnosisConclusion Conclusion { get; set; } = new DiagnosisConclusion();
        
        /// <summary>
        /// 诊断分析
        /// </summary>
        public DiagnosisAnalysis Analysis { get; set; } = new DiagnosisAnalysis();
        
        /// <summary>
        /// 鉴别诊断
        /// </summary>
        public List<DifferentialDiagnosis> DifferentialDiagnoses { get; set; } = new List<DifferentialDiagnosis>();
    }

    /// <summary>
    /// 诊断结论
    /// </summary>
    public class DiagnosisConclusion
    {
        /// <summary>
        /// 主要证候
        /// </summary>
        public string PrimarySyndrome { get; set; } = string.Empty;
        
        /// <summary>
        /// 伴随证候
        /// </summary>
        public List<string> AccompanyingSyndromes { get; set; } = new List<string>();
        
        /// <summary>
        /// 体质辨识
        /// </summary>
        public string ConstitutionType { get; set; } = string.Empty;
    }

    /// <summary>
    /// 诊断分析
    /// </summary>
    public class DiagnosisAnalysis
    {
        /// <summary>
        /// 主要依据
        /// </summary>
        public string PrimaryBasis { get; set; } = string.Empty;
        
        /// <summary>
        /// 伴随依据
        /// </summary>
        public string AccompanyingBasis { get; set; } = string.Empty;
        
        /// <summary>
        /// 体质影响
        /// </summary>
        public string ConstitutionInfluence { get; set; } = string.Empty;
    }

    /// <summary>
    /// 鉴别诊断
    /// </summary>
    public class DifferentialDiagnosis
    {
        /// <summary>
        /// 相似证候
        /// </summary>
        public string SimilarSyndrome { get; set; } = string.Empty;
        
        /// <summary>
        /// 关键区别
        /// </summary>
        public string KeyDifference { get; set; } = string.Empty;
    }

    /// <summary>
    /// 治疗与护理信息
    /// </summary>
    public class TreatmentCareInfo
    {
        /// <summary>
        /// 治疗建议
        /// </summary>
        public TreatmentRecommendation TreatmentRecommendation { get; set; } = new TreatmentRecommendation();
        
        /// <summary>
        /// 注意事项
        /// </summary>
        public Precautions Precautions { get; set; } = new Precautions();
    }

    /// <summary>
    /// 治疗建议
    /// </summary>
    public class TreatmentRecommendation
    {
        /// <summary>
        /// 治疗原则
        /// </summary>
        public string TreatmentPrinciple { get; set; } = string.Empty;
        
        /// <summary>
        /// 方药建议
        /// </summary>
        public string FormulaRecommendation { get; set; } = string.Empty;
        
        /// <summary>
        /// 饮食建议
        /// </summary>
        public string DietaryAdvice { get; set; } = string.Empty;
        
        /// <summary>
        /// 生活方式建议
        /// </summary>
        public string LifestyleAdvice { get; set; } = string.Empty;
    }

    /// <summary>
    /// 注意事项
    /// </summary>
    public class Precautions
    {
        /// <summary>
        /// 用药警示
        /// </summary>
        public string MedicationWarning { get; set; } = string.Empty;
        
        /// <summary>
        /// 复诊建议
        /// </summary>
        public string FollowUpAdvice { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dify Workflow API 响应数据结构（阻塞模式）
    /// </summary>
    public class DifyWorkflowResponse
    {
        [JsonPropertyName("workflow_run_id")]
        public string WorkflowRunId { get; set; } = string.Empty;

        [JsonPropertyName("task_id")]
        public string TaskId { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public DifyWorkflowData Data { get; set; } = new();
    }

    /// <summary>
    /// Dify Workflow 数据部分
    /// </summary>
    public class DifyWorkflowData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("workflow_id")]
        public string WorkflowId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("outputs")]
        public Dictionary<string, object>? Outputs { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("elapsed_time")]
        public float ElapsedTime { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

        [JsonPropertyName("total_steps")]
        public int TotalSteps { get; set; }

        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }

        [JsonPropertyName("finished_at")]
        public long FinishedAt { get; set; }
    }

    /// <summary>
    /// Dify API 错误响应
    /// </summary>
    public class DifyErrorResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }

    /// <summary>
    /// Dify Workflow 请求数据
    /// </summary>
    public class DifyWorkflowRequest
    {
        [JsonPropertyName("inputs")]
        public Dictionary<string, object> Inputs { get; set; } = new();

        [JsonPropertyName("response_mode")]
        public string ResponseMode { get; set; } = "blocking";

        [JsonPropertyName("user")]
        public string User { get; set; } = "doctor";
    }
}