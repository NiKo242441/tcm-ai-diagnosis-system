namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 避免食物DTO
    /// </summary>
    public class AvoidedFoodDto
    {
        /// <summary>
        /// 避免食物ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 食物名称
        /// </summary>
        public string FoodName { get; set; } = string.Empty;
    }
}