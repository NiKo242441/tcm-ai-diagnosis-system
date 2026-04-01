namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 饮食建议
    /// </summary>
    public class DietaryAdvice
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
        /// 饮食原则
        /// </summary>
        public string DietaryPrinciples { get; set; }

        /// <summary>
        /// 用餐时间
        /// </summary>
        public string MealTiming { get; set; }

        /// <summary>
        /// 烹饪方法
        /// </summary>
        public string CookingMethods { get; set; }

        /// <summary>
        /// 理论依据
        /// </summary>
        public string Rationale { get; set; }

        /// <summary>
        /// 季节调整
        /// </summary>
        public string SeasonalAdjustment { get; set; }

        /// <summary>
        /// 注意事项
        /// </summary>
        public string Precautions { get; set; }

        public virtual Treatment Treatment { get; set; }
        public virtual ICollection<RecommendedFood> RecommendedFoods { get; set; } = new List<RecommendedFood>();
        public virtual ICollection<AvoidedFood> AvoidedFoods { get; set; } = new List<AvoidedFood>();
    }
}