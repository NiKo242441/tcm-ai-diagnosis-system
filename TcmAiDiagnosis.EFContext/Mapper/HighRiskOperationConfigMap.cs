using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class HighRiskOperationConfigMap : IEntityTypeConfiguration<HighRiskOperationConfig>
    {
        public void Configure(EntityTypeBuilder<HighRiskOperationConfig> builder)
        {
            builder.ToTable("HighRiskOperationConfigs");
            builder.HasKey(x => x.Id);
        }
    }
}