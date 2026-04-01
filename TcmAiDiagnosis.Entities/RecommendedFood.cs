using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 推荐食物
    /// </summary>
    public class RecommendedFood
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DietaryAdviceId { get; set; }

        /// <summary>
        /// 食物名称
        /// </summary>
        [Required]
        public string FoodName { get; set; }

        [ForeignKey("DietaryAdviceId")]
        public virtual DietaryAdvice DietaryAdvice { get; set; }
    }
}