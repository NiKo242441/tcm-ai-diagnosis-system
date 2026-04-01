using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class TreatmentChangeLogMap : IEntityTypeConfiguration<TreatmentChangeLog>
    {
        public void Configure(EntityTypeBuilder<TreatmentChangeLog> builder)
        {
            builder.HasKey(tcl => tcl.Id);
            builder.Property(tcl => tcl.ChangeType).IsRequired();
            builder.Property(tcl => tcl.ChangeDescription).IsRequired();

            builder.HasOne(tcl => tcl.Treatment)
                .WithMany(t => t.TreatmentChangeLogs)
                .HasForeignKey(tcl => tcl.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tcl => tcl.ChangedByUser)
                .WithMany()
                .HasForeignKey(tcl => tcl.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}