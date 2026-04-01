namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 草药使用警告
    /// </summary>
    public class HerbalWarning
    {
        public int Id { get; set; }
        public int TreatmentId { get; set; }

        /// <summary>
        /// 警告类型
        /// </summary>
        public string WarningType { get; set; }

        /// <summary>
        /// 警告标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 警告内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 严重程度
        /// </summary>
        public string SeverityLevel { get; set; }

        /// <summary>
        /// 需观察症状
        /// </summary>
        public string SymptomsToWatch { get; set; }

        /// <summary>
        /// 需采取的行动
        /// </summary>
        public string ActionRequired { get; set; }

        /// <summary>
        /// 预防措施
        /// </summary>
        public string PreventionMeasures { get; set; }

        /// <summary>
        /// 特殊人群
        /// </summary>
        public string SpecialPopulations { get; set; }

        public virtual Treatment Treatment { get; set; }
        public virtual ICollection<AffectedMedication> AffectedMedications { get; set; } = new List<AffectedMedication>();
    }
}