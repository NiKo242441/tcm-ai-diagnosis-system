namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 拔罐治疗记录DTO - 对应每一个部位的拔罐操作记录
    /// </summary>
    public class CuppingDto
    {
        /// <summary>
        /// 拔罐记录ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 拔罐部位
        /// </summary>
        public string Area { get; set; } = string.Empty;

        /// <summary>
        /// 具体穴位
        /// </summary>
        public string SpecificPoints { get; set; } = string.Empty;

        /// <summary>
        /// 适用症状
        /// </summary>
        public string SuitableFor { get; set; } = string.Empty;

        /// <summary>
        /// 拔罐方法
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// 罐具类型
        /// </summary>
        public string CupType { get; set; } = string.Empty;

        /// <summary>
        /// 罐具大小
        /// </summary>
        public string CupSize { get; set; } = string.Empty;

        /// <summary>
        /// 吸力强度
        /// </summary>
        public string SuctionStrength { get; set; } = string.Empty;

        /// <summary>
        /// 留罐时间
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
        public string TechniquePoints { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        public string Precautions { get; set; } = string.Empty;

        /// <summary>
        /// 患者友好名称
        /// </summary>
        public string? PatientFriendlyName { get; set; }
    }
}