using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class FollowUpAdviceMap : IEntityTypeConfiguration<FollowUpAdvice>
    {
        public void Configure(EntityTypeBuilder<FollowUpAdvice> builder)
        {
            builder.HasKey(fa => fa.Id);

            builder.Property(fa => fa.FollowUpType).IsRequired().HasMaxLength(64);
            builder.Property(fa => fa.Title).IsRequired().HasMaxLength(128);
            builder.Property(fa => fa.Timing).IsRequired().HasMaxLength(256);
            builder.Property(fa => fa.Purpose).IsRequired();
            builder.Property(fa => fa.PreparationRequired).IsRequired();
            builder.Property(fa => fa.EmergencyConditions).IsRequired();
            builder.Property(fa => fa.SelfMonitoring).IsRequired();
            builder.Property(fa => fa.ContactInformation).IsRequired().HasMaxLength(256);

            builder.HasOne(fa => fa.Treatment)
                .WithMany(t => t.FollowUpAdvices)
                .HasForeignKey(fa => fa.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(fa => fa.MonitoringIndicators)
                .WithOne(mi => mi.FollowUpAdvice)
                .HasForeignKey(mi => mi.FollowUpAdviceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}