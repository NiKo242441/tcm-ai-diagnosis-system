namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 食疗方
    /// </summary>
    public class DietaryTherapy
    {
        public int Id { get; set; }
        public int TreatmentId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Preparation { get; set; }
        public string Efficacy { get; set; }
        public string SuitableFor { get; set; }
        public string Contraindications { get; set; }
        public string ServingMethod { get; set; }
        public string StorageMethod { get; set; }
        public string? PatientFriendlyName { get; set; }

        public virtual Treatment Treatment { get; set; }
        public virtual ICollection<DietaryTherapyIngredient> DietaryTherapyIngredients { get; set; } = new List<DietaryTherapyIngredient>();
    }
}