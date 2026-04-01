namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 针灸治疗
    /// </summary>
    public class Acupuncture
    {
        public int Id { get; set; }
        public int TreatmentId { get; set; }
        public string PointName { get; set; }
        public string Location { get; set; }
        public string Method { get; set; }
        public string Technique { get; set; }
        public string NeedleSpecification { get; set; }
        public string Depth { get; set; }
        public string Duration { get; set; }
        public string Frequency { get; set; }
        public string Efficacy { get; set; }
        public string Indications { get; set; }
        public string Instructions { get; set; }
        public string Notes { get; set; }
        public string Contraindications { get; set; }
        public string? PatientFriendlyName { get; set; }
        public string? PatientFriendlyDescription { get; set; }
        public string? InstructionVideoUrl { get; set; }

        public virtual Treatment Treatment { get; set; }
    }
}