using Admin.Quartz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Admin.EntityTypeConfigurations
{
    public class QrtzCronTriggerEntityTypeConfiguration : IEntityTypeConfiguration<QrtzCronTrigger>
    {
        public void Configure(EntityTypeBuilder<QrtzCronTrigger> builder)
        {
            builder.ToTable("QrtzCronTriggers", AdminConsts.DbSchema, e => e.HasComment("Quartz QrtzCronTriggers"));

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.Property(x => x.CronExpression)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.TimeZoneId)
                .HasMaxLength(100);

            builder.HasIndex(x => x.TriggerId)
                .IsUnique();

            builder.ConfigureFullAuditedAggregateRoot();
            builder.ConfigureByConvention();

        }
    }
}
