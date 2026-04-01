namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// Dify API 配置选项
    /// </summary>
    public class DifyApiOptions
    {
        /// <summary>
        /// 配置节名称
        /// </summary>
        public const string SectionName = "DifyApi";

        /// <summary>
        /// API 基础地址
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// API 密钥（用于基础API访问）
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// 证候概览工作流API密钥
        /// </summary>
        public string OverviewWorkflowApiKey { get; set; } = string.Empty;

        /// <summary>
        /// 证候详情工作流API密钥
        /// </summary>
        public string DetailWorkflowApiKey { get; set; } = string.Empty;

        /// <summary>
        /// 治疗方案工作流API密钥
        /// </summary>
        public string TreatmentWorkflowApiKey { get; set; } = string.Empty;

        /// <summary>
        /// 证候概览工作流端点
        /// </summary>
        public string OverviewEndpoint { get; set; } = "/v1/workflows/run";

        /// <summary>
        /// 证候详情工作流端点
        /// </summary>
        public string DetailEndpoint { get; set; } = "/v1/workflows/run";

        /// <summary>
        /// 治疗方案工作流端点
        /// </summary>
        public string TreatmentEndpoint { get; set; } = "/v1/workflows/run";

        /// <summary>
        /// 请求超时时间（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 响应模式：blocking（阻塞）或 streaming（流式）
        /// </summary>
        public string ResponseMode { get; set; } = "blocking";

        /// <summary>
        /// 用户标识
        /// </summary>
        public string User { get; set; } = "doctor";

        /// <summary>
        /// 获取完整的概览 API 地址
        /// </summary>
        public string OverviewUrl => $"{BaseUrl.TrimEnd('/')}{OverviewEndpoint}";

        /// <summary>
        /// 获取完整的详情 API 地址
        /// </summary>
        public string DetailUrl => $"{BaseUrl.TrimEnd('/')}{DetailEndpoint}";

        /// <summary>
        /// 获取完整的治疗方案 API 地址
        /// </summary>
        public string TreatmentUrl => $"{BaseUrl.TrimEnd('/')}{TreatmentEndpoint}";

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        /// <returns>配置是否有效</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(BaseUrl) &&
                   !string.IsNullOrWhiteSpace(OverviewWorkflowApiKey) &&
                   !string.IsNullOrWhiteSpace(DetailWorkflowApiKey) &&
                   !string.IsNullOrWhiteSpace(TreatmentWorkflowApiKey) &&
                   !string.IsNullOrWhiteSpace(OverviewEndpoint) &&
                   !string.IsNullOrWhiteSpace(DetailEndpoint) &&
                   !string.IsNullOrWhiteSpace(TreatmentEndpoint) &&
                   TimeoutSeconds > 0 &&
                   (ResponseMode == "blocking" || ResponseMode == "streaming") &&
                   !string.IsNullOrWhiteSpace(User);
        }
    }
}