namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 推荐食物DTO
    /// </summary>
    public class RecommendedFoodDto
    {
        /// <summary>
        /// 推荐食物ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 食物名称
        /// </summary>
        public string FoodName { get; set; } = string.Empty;
    }
}