namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 生活方式建议DTO
    /// </summary>
    public class LifestyleAdviceDto
    {
        /// <summary>
        /// 建议ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 建议类别
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 建议标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 建议内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 理论依据
        /// </summary>
        public string Rationale { get; set; } = string.Empty;

        /// <summary>
        /// 实施方法
        /// </summary>
        public string Implementation { get; set; } = string.Empty;

        /// <summary>
        /// 执行频率
        /// </summary>
        public string Frequency { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        public string Precautions { get; set; } = string.Empty;

        /// <summary>
        /// 预期效果
        /// </summary>
        public string Benefits { get; set; } = string.Empty;
    }
}