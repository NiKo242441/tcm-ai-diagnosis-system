namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 针刺治疗记录DTO - 对应每一个穴位的针刺操作记录
    /// </summary>
    public class AcupunctureDto
    {
        /// <summary>
        /// 针刺记录ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 穴位名称
        /// </summary>
        public string PointName { get; set; } = string.Empty;

        /// <summary>
        /// 穴位定位
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// 针刺方法
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// 操作手法
        /// </summary>
        public string Technique { get; set; } = string.Empty;

        /// <summary>
        /// 针具规格
        /// </summary>
        public string NeedleSpecification { get; set; } = string.Empty;

        /// <summary>
        /// 针刺深度
        /// </summary>
        public string Depth { get; set; } = string.Empty;

        /// <summary>
        /// 留针时间
        /// </summary>
        public string Duration { get; set; } = string.Empty;

        /// <summary>
        /// 治疗频率
        /// </summary>
        public string Frequency { get; set; } = string.Empty;

        /// <summary>
        /// 功效
        /// </summary>
        public string Efficacy { get; set; } = string.Empty;

        /// <summary>
        /// 主治
        /// </summary>
        public string Indications { get; set; } = string.Empty;

        /// <summary>
        /// 操作要点
        /// </summary>
        public string Instructions { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// 禁忌
        /// </summary>
        public string Contraindications { get; set; } = string.Empty;

        /// <summary>
        /// 患者友好名称
        /// </summary>
        public string? PatientFriendlyName { get; set; }

        /// <summary>
        /// 通俗化描述
        /// </summary>
        public string? PatientFriendlyDescription { get; set; }
    }
}