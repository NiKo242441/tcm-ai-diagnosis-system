namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 随访建议
    /// </summary>
    public class FollowUpAdvice
    {
        public int Id { get; set; }
        public int TreatmentId { get; set; }

        /// <summary>
        /// 随访类型
        /// </summary>
        public string FollowUpType { get; set; }

        /// <summary>
        /// 随访标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 随访时间
        /// </summary>
        public string Timing { get; set; }

        /// <summary>
        /// 随访目的
        /// </summary>
        public string Purpose { get; set; }

        /// <summary>
        /// 准备事项
        /// </summary>
        public string PreparationRequired { get; set; }

        /// <summary>
        /// 紧急情况
        /// </summary>
        public string EmergencyConditions { get; set; }

        /// <summary>
        /// 自我监测
        /// </summary>
        public string SelfMonitoring { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        public string ContactInformation { get; set; }

        public virtual Treatment Treatment { get; set; }
        public virtual ICollection<MonitoringIndicator> MonitoringIndicators { get; set; } = new List<MonitoringIndicator>();
    }
}