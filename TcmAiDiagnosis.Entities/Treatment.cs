using TcmAiDiagnosis.Entities.Enums;
using System;
using System.Collections.Generic;

namespace TcmAiDiagnosis.Entities
{
    public class Treatment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int VisitId { get; set; }
        public int SyndromeId { get; set; }
        public string TcmDiagnosis { get; set; }
        public string SyndromeAnalysis { get; set; }
        public string TreatmentPrinciple { get; set; }
        public string ExpectedOutcome { get; set; }
        public string Precautions { get; set; }
        public TreatmentStatus Status { get; set; }
        public string Version { get; set; }
        public bool IsLatest { get; set; }
        public bool IsAiOriginated { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public int? ArchivedByUserId { get; set; }
        public int TenantId { get; set; }

        // Navigation properties
        public User Patient { get; set; }
        public Visit Visit { get; set; }
        public Syndrome Syndrome { get; set; }

        // Collections
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public virtual ICollection<Acupuncture> Acupunctures { get; set; } = new List<Acupuncture>();
        public virtual ICollection<Moxibustion> Moxibustions { get; set; } = new List<Moxibustion>();
        public virtual ICollection<Cupping> Cuppings { get; set; } = new List<Cupping>();
        public virtual ICollection<DietaryTherapy> DietaryTherapies { get; set; } = new List<DietaryTherapy>();
        public virtual ICollection<LifestyleAdvice> LifestyleAdvices { get; set; } = new List<LifestyleAdvice>();
        public virtual ICollection<DietaryAdvice> DietaryAdvices { get; set; } = new List<DietaryAdvice>();
        public virtual ICollection<FollowUpAdvice> FollowUpAdvices { get; set; } = new List<FollowUpAdvice>();
        public virtual ICollection<HerbalWarning> HerbalWarnings { get; set; } = new List<HerbalWarning>();
        public virtual ICollection<TreatmentVersion> TreatmentVersions { get; set; } = new List<TreatmentVersion>();
        public virtual ICollection<TreatmentChangeLog> TreatmentChangeLogs { get; set; } = new List<TreatmentChangeLog>();
    }
}