namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 监测指标
    /// </summary>
    public class MonitoringIndicator
    {
        public int Id { get; set; }
        public int FollowUpAdviceId { get; set; }

        /// <summary>
        /// 指标名称
        /// </summary>
        public string IndicatorName { get; set; }

        public virtual FollowUpAdvice FollowUpAdvice { get; set; }
    }
}