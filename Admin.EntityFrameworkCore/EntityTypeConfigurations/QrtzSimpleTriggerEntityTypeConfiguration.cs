using Admin.Quartz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Admin.EntityTypeConfigurations
{
    public class QrtzSimpleTriggerEntityTypeConfiguration : IEntityTypeConfiguration<QrtzSimpleTrigger>
    {
        public void Configure(EntityTypeBuilder<QrtzSimpleTrigger> builder)
        {
            builder.ToTable("QrtzSimpleTriggers", AdminConsts.DbSchema, e => e.HasComment("Quartz QrtzSimpleTriggers"));

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.Property(x => x.RepeatCount).IsRequired();
            builder.Property(x => x.IntervalSeconds).IsRequired();

            builder.HasIndex(x => x.TriggerId)
                .IsUnique();

            builder.ConfigureFullAuditedAggregateRoot();
            builder.ConfigureByConvention();
        }
    }
}
