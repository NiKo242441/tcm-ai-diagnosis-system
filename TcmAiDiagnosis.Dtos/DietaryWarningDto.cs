namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 食疗安全警告DTO
    /// </summary>
    public class DietaryWarningDto
    {
        /// <summary>
        /// 警告ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 警告类型
        /// </summary>
        public string WarningType { get; set; } = string.Empty;

        /// <summary>
        /// 警告标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 警告内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 严重程度
        /// </summary>
        public string SeverityLevel { get; set; } = string.Empty;
    }
}