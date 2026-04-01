using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class MonitoringIndicatorMap : IEntityTypeConfiguration<MonitoringIndicator>
    {
        public void Configure(EntityTypeBuilder<MonitoringIndicator> builder)
        {
            builder.HasKey(mi => mi.Id);
            builder.Property(mi => mi.IndicatorName).IsRequired().HasMaxLength(128);
        }
    }
}