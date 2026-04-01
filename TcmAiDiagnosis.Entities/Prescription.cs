namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 处方
    /// </summary>
    public class Prescription
    {
        public int Id { get; set; }
        public int TreatmentId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }
        public string Efficacy { get; set; }
        public string Indications { get; set; }
        public string Contraindications { get; set; }
        public string Notes { get; set; }
        public string? PatientFriendlyName { get; set; }
        public string? PatientFriendlyDescription { get; set; }
        public string? InstructionVideoUrl { get; set; }

        public virtual Treatment Treatment { get; set; }
        public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}