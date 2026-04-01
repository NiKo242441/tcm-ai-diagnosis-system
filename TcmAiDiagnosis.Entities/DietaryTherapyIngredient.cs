namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 食疗方原料
    /// </summary>
    public class DietaryTherapyIngredient
    {
        public int Id { get; set; }
        public int DietaryTherapyId { get; set; }
        public string Name { get; set; }
        public string Dosage { get; set; }
        public string ProcessingMethod { get; set; }
        public string Notes { get; set; }

        public virtual DietaryTherapy DietaryTherapy { get; set; }
    }
}