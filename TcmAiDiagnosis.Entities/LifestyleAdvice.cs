namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 生活方式建议
    /// </summary>
    public class LifestyleAdvice
    {
        public int Id { get; set; }
        public int TreatmentId { get; set; }

        /// <summary>
        /// 建议类别
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 建议标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 建议内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 理论依据
        /// </summary>
        public string Rationale { get; set; }

        /// <summary>
        /// 实施方法
        /// </summary>
        public string Implementation { get; set; }

        /// <summary>
        /// 执行频率
        /// </summary>
        public string Frequency { get; set; }

        /// <summary>
        /// 注意事项
        /// </summary>
        public string Precautions { get; set; }

        /// <summary>
        /// 预期效果
        /// </summary>
        public string Benefits { get; set; }

        public virtual Treatment Treatment { get; set; }
    }
}