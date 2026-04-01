namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 受影响的药物
    /// </summary>
    public class AffectedMedication
    {
        public int Id { get; set; }
        public int HerbalWarningId { get; set; }

        /// <summary>
        /// 药物名称
        /// </summary>
        public string MedicationName { get; set; }

        public virtual HerbalWarning HerbalWarning { get; set; }
    }
}