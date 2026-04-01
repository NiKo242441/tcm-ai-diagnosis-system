namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 中药安全警告DTO - 结构修正版本，使用结构化的子DTO列表
    /// </summary>
    public class HerbalWarningDto
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

        /// <summary>
        /// 需观察症状
        /// </summary>
        public string SymptomsToWatch { get; set; } = string.Empty;

        /// <summary>
        /// 需采取的行动
        /// </summary>
        public string ActionRequired { get; set; } = string.Empty;

        /// <summary>
        /// 预防措施
        /// </summary>
        public string PreventionMeasures { get; set; } = string.Empty;

        /// <summary>
        /// 特殊人群
        /// </summary>
        public string SpecialPopulations { get; set; } = string.Empty;

        // --- 结构修正：使用结构化的子DTO列表 ---
        /// <summary>
        /// 涉及药物列表
        /// </summary>
        public List<AffectedMedicationDto> AffectedMedications { get; set; } = new();
    }
}