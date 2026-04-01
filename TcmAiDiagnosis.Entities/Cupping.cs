namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 拔罐治疗
    /// </summary>
    public class Cupping
    {
        public int Id { get; set; }
        public int TreatmentId { get; set; }
        public string Area { get; set; }
        public string SpecificPoints { get; set; }
        public string SuitableFor { get; set; }
        public string Method { get; set; }
        public string CupType { get; set; }
        public string CupSize { get; set; }
        public string SuctionStrength { get; set; }
        public string Duration { get; set; }
        public string Frequency { get; set; }
        public string Efficacy { get; set; }
        public string Indications { get; set; }
        public string TechniquePoints { get; set; }
        public string Precautions { get; set; }
        public string? PatientFriendlyName { get; set; }

        public virtual Treatment Treatment { get; set; }
    }
}