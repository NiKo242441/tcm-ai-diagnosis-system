namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 处方项
    /// </summary>
    public class PrescriptionItem
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public string Name { get; set; }
        public string Dosage { get; set; }
        public string Unit { get; set; }
        public string ProcessingMethod { get; set; }
        public string Notes { get; set; }

        public virtual Prescription Prescription { get; set; }
    }
}