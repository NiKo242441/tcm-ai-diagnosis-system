namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 饮食建议DTO - 结构修正版本，使用结构化的子DTO列表
    /// </summary>
    public class DietaryAdviceDto
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
        /// 饮食原则
        /// </summary>
        public string DietaryPrinciples { get; set; } = string.Empty;

        /// <summary>
        /// 用餐时间
        /// </summary>
        public string MealTiming { get; set; } = string.Empty;

        /// <summary>
        /// 烹饪方法
        /// </summary>
        public string CookingMethods { get; set; } = string.Empty;

        /// <summary>
        /// 理论依据
        /// </summary>
        public string Rationale { get; set; } = string.Empty;

        /// <summary>
        /// 季节调整
        /// </summary>
        public string SeasonalAdjustment { get; set; } = string.Empty;

        /// <summary>
        /// 注意事项
        /// </summary>
        public string Precautions { get; set; } = string.Empty;

        // --- 结构修正：使用结构化的子DTO列表 ---
        /// <summary>
        /// 推荐食物列表
        /// </summary>
        public List<RecommendedFoodDto> RecommendedFoods { get; set; } = new();

        /// <summary>
        /// 避免食物列表
        /// </summary>
        public List<AvoidedFoodDto> AvoidedFoods { get; set; } = new();
    }
}