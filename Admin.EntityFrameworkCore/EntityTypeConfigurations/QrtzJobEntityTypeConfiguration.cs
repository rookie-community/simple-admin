using Admin.Quartz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Admin.EntityTypeConfigurations
{
    public class QrtzJobEntityTypeConfiguration : IEntityTypeConfiguration<QrtzJob>
    {
        public void Configure(EntityTypeBuilder<QrtzJob> builder)
        {
            builder.ToTable("QrtzJobs", AdminConsts.DbSchema, e => e.HasComment("Quartz Jobs"));

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.Property(x => x.JobName)
                    .IsRequired()
                    .HasMaxLength(100);

            builder.Property(x => x.JobGroup)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.JobClassName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasIndex(x => new { x.JobName, x.JobGroup })
                .IsUnique();

            builder.ConfigureFullAuditedAggregateRoot();
            builder.ConfigureByConvention();
        }
    }
}
