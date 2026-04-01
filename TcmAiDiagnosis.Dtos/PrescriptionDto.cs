namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 中药处方DTO
    /// </summary>
    public class PrescriptionDto
    {
        /// <summary>
        /// 处方ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 方剂名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 方剂类别
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 方剂描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 用法用量
        /// </summary>
        public string Usage { get; set; } = string.Empty;

        /// <summary>
        /// 用药副数（一般一天一副药，设置开多少副）
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// 功效
        /// </summary>
        public string Efficacy { get; set; } = string.Empty;

        /// <summary>
        /// 主治
        /// </summary>
        public string Indications { get; set; } = string.Empty;

        /// <summary>
        /// 禁忌
        /// </summary>
        public string Contraindications { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// 患者友好名称
        /// </summary>
        public string? PatientFriendlyName { get; set; }

        /// <summary>
        /// 通俗化描述
        /// </summary>
        public string? PatientFriendlyDescription { get; set; }

        /// <summary>
        /// 处方药材明细列表
        /// </summary>
        public List<PrescriptionItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// 处方药材明细DTO
    /// </summary>
    public class PrescriptionItemDto
    {
        /// <summary>
        /// 药材明细ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 药材名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 用量 (强制命名)
        /// </summary>
        public string Dosage { get; set; } = string.Empty;

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// 炮制方法
        /// </summary>
        public string ProcessingMethod { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string Notes { get; set; } = string.Empty;
    }
}