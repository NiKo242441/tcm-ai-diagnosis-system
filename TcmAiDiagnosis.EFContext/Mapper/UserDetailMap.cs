using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;


namespace TcmAiDiagnosis.EFContext.Mapper
{
    internal class UserDetailMap : IEntityTypeConfiguration<UserDetail>
    {
        public void Configure(EntityTypeBuilder<UserDetail> builder)
        {
            builder.HasKey(x => x.UserId);
            builder.Property(x => x.UserId).ValueGeneratedNever(); // 手动指定UserId，不自动生成
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)").ValueGeneratedOnUpdate();
            
            // 外键关系配置
            builder.HasOne(x => x.User).WithOne(x => x.Detail).HasForeignKey<UserDetail>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
