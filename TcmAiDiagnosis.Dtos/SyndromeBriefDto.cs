namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 证候摘要信息DTO
    /// </summary>
    public class SyndromeBriefDto
    {
        /// <summary>
        /// 证候ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 证候名称
        /// </summary>
        public string SyndromeName { get; set; } = string.Empty;

        /// <summary>
        /// 置信度
        /// </summary>
        public decimal Confidence { get; set; }

        /// <summary>
        /// 主要症状
        /// </summary>
        public string MainSymptoms { get; set; } = string.Empty;

        /// <summary>
        /// 治疗原则
        /// </summary>
        public string TreatmentPrinciple { get; set; } = string.Empty;
    }
}