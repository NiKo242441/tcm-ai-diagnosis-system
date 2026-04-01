namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 食疗方案DTO
    /// </summary>
    public class DietaryTherapyDto
    {
        /// <summary>
        /// 食疗方案ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 食疗方名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 食疗类别
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 食疗方描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 制作方法
        /// </summary>
        public string Preparation { get; set; } = string.Empty;

        /// <summary>
        /// 功效
        /// </summary>
        public string Efficacy { get; set; } = string.Empty;

        /// <summary>
        /// 适用人群
        /// </summary>
        public string SuitableFor { get; set; } = string.Empty;

        /// <summary>
        /// 禁忌
        /// </summary>
        public string Contraindications { get; set; } = string.Empty;

        /// <summary>
        /// 服用方法
        /// </summary>
        public string ServingMethod { get; set; } = string.Empty;

        /// <summary>
        /// 保存方法
        /// </summary>
        public string StorageMethod { get; set; } = string.Empty;

        /// <summary>
        /// 患者友好名称
        /// </summary>
        public string? PatientFriendlyName { get; set; }

        /// <summary>
        /// 食疗方案成分列表
        /// </summary>
        public List<DietaryTherapyIngredientDto> Ingredients { get; set; } = new();
    }

    /// <summary>
    /// 食疗方案成分DTO
    /// </summary>
    public class DietaryTherapyIngredientDto
    {
        /// <summary>
        /// 成分ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 食材名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 用量 (强制命名)
        /// </summary>
        public string Dosage { get; set; } = string.Empty;

        /// <summary>
        /// 处理方法
        /// </summary>
        public string ProcessingMethod { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string Notes { get; set; } = string.Empty;
    }
}