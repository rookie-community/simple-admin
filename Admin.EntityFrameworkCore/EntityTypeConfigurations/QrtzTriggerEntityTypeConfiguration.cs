using Admin.Quartz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Admin.EntityTypeConfigurations
{
    public class QrtzTriggerEntityTypeConfiguration : IEntityTypeConfiguration<QrtzTrigger>
    {
        public void Configure(EntityTypeBuilder<QrtzTrigger> builder)
        {
            builder.ToTable("QrtzTriggers", AdminConsts.DbSchema, e => e.HasComment("Quartz QrtzTriggers"));

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.Property(x => x.TriggerName)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.TriggerGroup)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.TriggerType)
                .IsRequired()
                .HasMaxLength(30)
                .HasConversion<string>();

            builder.Property(x => x.Priority)
                .HasDefaultValue(5);
            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasIndex(x => new { x.TriggerName, x.TriggerGroup })
                .IsUnique();

            // Job 关联
            builder.HasOne(x => x.Job)
                .WithMany(x => x.Triggers)
                .HasForeignKey(x => x.JobId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // 子表关联（一对一）
            builder.HasOne(x => x.CronTrigger)
                .WithOne(x => x.Trigger)
                .HasForeignKey<QrtzCronTrigger>(x => x.TriggerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SimpleTrigger)
                .WithOne(x => x.Trigger)
                .HasForeignKey<QrtzSimpleTrigger>(x => x.TriggerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SimPropTrigger)
                .WithOne(x => x.Trigger)
                .HasForeignKey<QrtzSimPropTrigger>(x => x.TriggerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ConfigureFullAuditedAggregateRoot();
            builder.ConfigureByConvention();
        }
    }
}
