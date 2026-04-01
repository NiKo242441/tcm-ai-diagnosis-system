namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 监测指标DTO
    /// </summary>
    public class MonitoringIndicatorDto
    {
        /// <summary>
        /// 监测指标ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 指标名称
        /// </summary>
        public string IndicatorName { get; set; } = string.Empty;
    }
}