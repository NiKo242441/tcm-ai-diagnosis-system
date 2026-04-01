namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 艾灸治疗记录DTO - 对应每一个穴位的艾灸操作记录
    /// </summary>
    public class MoxibustionDto
    {
        /// <summary>
        /// 艾灸记录ID
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
        /// 艾灸方法
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// 艾灸材料
        /// </summary>
        public string MoxaType { get; set; } = string.Empty;

        /// <summary>
        /// 操作手法
        /// </summary>
        public string Technique { get; set; } = string.Empty;

        /// <summary>
        /// 温度控制
        /// </summary>
        public string TemperatureControl { get; set; } = string.Empty;

        /// <summary>
        /// 艾灸时间
        /// </summary>
        public string Duration { get; set; } = string.Empty;

        /// <summary>
        /// 治疗频率
        /// </summary>
        public string Frequency { get; set; } = string.Empty;

        /// <summary>
        /// 疗程时长
        /// </summary>
        public string CourseDuration { get; set; } = string.Empty;

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
        public string TechniquePoints { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        public string Precautions { get; set; } = string.Empty;

        /// <summary>
        /// 禁忌
        /// </summary>
        public string Contraindications { get; set; } = string.Empty;

        /// <summary>
        /// 治疗后护理
        /// </summary>
        public string PostTreatmentCare { get; set; } = string.Empty;

        /// <summary>
        /// 配合治疗
        /// </summary>
        public string CombinationTherapy { get; set; } = string.Empty;

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