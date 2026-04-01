namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 艾灸治疗
    /// </summary>
    public class Moxibustion
    {
        public int Id { get; set; }
        public int TreatmentId { get; set; }
        public string PointName { get; set; }
        public string Location { get; set; }
        public string Method { get; set; }
        public string MoxaType { get; set; }
        public string Technique { get; set; }
        public string TemperatureControl { get; set; }
        public string Duration { get; set; }
        public string Frequency { get; set; }
        public string CourseDuration { get; set; }
        public string Efficacy { get; set; }
        public string Indications { get; set; }
        public string TechniquePoints { get; set; }
        public string Precautions { get; set; }
        public string Contraindications { get; set; }
        public string PostTreatmentCare { get; set; }
        public string CombinationTherapy { get; set; }
        public string? PatientFriendlyName { get; set; }
        public string? PatientFriendlyDescription { get; set; }

        public virtual Treatment Treatment { get; set; }
    }
}