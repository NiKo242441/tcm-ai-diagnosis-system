namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 随访建议DTO - 结构修正版本，使用结构化的子DTO列表
    /// </summary>
    public class FollowUpAdviceDto
    {
        /// <summary>
        /// 随访建议ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 随访类型
        /// </summary>
        public string FollowUpType { get; set; } = string.Empty;

        /// <summary>
        /// 随访标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 随访时间
        /// </summary>
        public string Timing { get; set; } = string.Empty;

        /// <summary>
        /// 随访目的
        /// </summary>
        public string Purpose { get; set; } = string.Empty;

        /// <summary>
        /// 准备事项
        /// </summary>
        public string PreparationRequired { get; set; } = string.Empty;

        /// <summary>
        /// 紧急情况
        /// </summary>
        public string EmergencyConditions { get; set; } = string.Empty;

        /// <summary>
        /// 自我监测
        /// </summary>
        public string SelfMonitoring { get; set; } = string.Empty;

        /// <summary>
        /// 联系方式
        /// </summary>
        public string ContactInformation { get; set; } = string.Empty;

        // --- 结构修正：使用结构化的子DTO列表 ---
        /// <summary>
        /// 监测指标列表
        /// </summary>
        public List<MonitoringIndicatorDto> MonitoringIndicators { get; set; } = new();
    }
}