using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext
{
    public class TcmAiDiagnosisContext : IdentityDbContext<User, Role, int>
    {
        /// <summary>
        /// 医生-患者关联表
        /// </summary>
        public DbSet<DoctorPatient> DoctorPatients { get; set; }

        /// <summary>
        /// 患者-租户关联表
        /// </summary>
        public DbSet<PatientTenant> PatientTenants { get; set; }

        // 实体类对应的数据库表集合
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Renewal> Renewals { get; set; }
        public DbSet<Syndrome> Syndromes { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<VisitSeries> VisitSeries { get; set; }

        /// <summary>
        /// 中药材表
        /// </summary>
        public DbSet<Herb> Herbs { get; set; }

        /// <summary>
        /// 中药配伍禁忌表
        /// </summary>
        public DbSet<HerbContraindication> HerbContraindications { get; set; }

        public DbSet<Treatment> Treatments { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<Acupuncture> Acupunctures { get; set; }
        public DbSet<Moxibustion> Moxibustions { get; set; }
        public DbSet<Cupping> Cuppings { get; set; }
        public DbSet<DietaryTherapy> DietaryTherapies { get; set; }
        public DbSet<DietaryTherapyIngredient> DietaryTherapyIngredients { get; set; }
        public DbSet<LifestyleAdvice> LifestyleAdvices { get; set; }
        public DbSet<DietaryAdvice> DietaryAdvices { get; set; }
        public DbSet<RecommendedFood> RecommendedFoods { get; set; }
        public DbSet<AvoidedFood> AvoidedFoods { get; set; }
        public DbSet<FollowUpAdvice> FollowUpAdvices { get; set; }
        public DbSet<MonitoringIndicator> MonitoringIndicators { get; set; }
        public DbSet<HerbalWarning> HerbalWarnings { get; set; }
        public DbSet<AffectedMedication> AffectedMedications { get; set; }
        public DbSet<TreatmentVersion> TreatmentVersions { get; set; }
        public DbSet<TreatmentChangeLog> TreatmentChangeLogs { get; set; }

        public DbSet<DoctorVerificationRecord> DoctorVerificationRecords { get; set; }
        public DbSet<HighRiskOperationConfig> HighRiskOperationConfigs { get; set; }
        public DbSet<TreatmentTemplate> TreatmentTemplates { get; set; }

        // 库存管理相关实体
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryOperation> InventoryOperations { get; set; }
        public DbSet<InventoryAlertRule> InventoryAlertRules { get; set; }

        // 审计日志
        public DbSet<AuditLog> AuditLogs { get; set; }

        // 权限管理相关实体
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<TemporaryPermission> TemporaryPermissions { get; set; }
        public DbSet<PermissionChangeLog> PermissionChangeLogs { get; set; }

        public TcmAiDiagnosisContext(DbContextOptions<TcmAiDiagnosisContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 自动应用所有EntityTypeConfiguration配置
            builder.ApplyConfigurationsFromAssembly(typeof(TcmAiDiagnosisContext).Assembly);

            base.OnModelCreating(builder);
        }
    }
}