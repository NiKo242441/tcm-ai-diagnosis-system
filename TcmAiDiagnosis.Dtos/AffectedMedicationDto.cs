namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 涉及药物DTO
    /// </summary>
    public class AffectedMedicationDto
    {
        /// <summary>
        /// 涉及药物ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 药物名称
        /// </summary>
        public string MedicationName { get; set; } = string.Empty;
    }
}