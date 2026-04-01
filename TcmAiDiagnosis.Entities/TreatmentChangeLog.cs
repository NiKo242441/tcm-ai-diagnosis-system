namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 治疗方案变更日志
    /// </summary>
    public class TreatmentChangeLog
    {
        public int Id { get; set; }
        public int TreatmentId { get; set; }
        public string ChangeType { get; set; }
        public string ChangeDescription { get; set; }
        public DateTime ChangedAt { get; set; }
        public int ChangedByUserId { get; set; }

        public virtual Treatment Treatment { get; set; }
        public virtual User ChangedByUser { get; set; }
    }
}