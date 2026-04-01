namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 治疗方案版本
    /// </summary>
    public class TreatmentVersion
    {
        public int Id { get; set; }
        public int TreatmentId { get; set; }
        public string Version { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }

        public virtual Treatment Treatment { get; set; }
        public virtual User CreatedByUser { get; set; }
    }
}