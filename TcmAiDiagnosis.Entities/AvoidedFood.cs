namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 忌口食物
    /// </summary>
    public class AvoidedFood
    {
        public int Id { get; set; }
        public int DietaryAdviceId { get; set; }

        /// <summary>
        /// 食物名称
        /// </summary>
        public string FoodName { get; set; }

        public virtual DietaryAdvice DietaryAdvice { get; set; }
    }
}