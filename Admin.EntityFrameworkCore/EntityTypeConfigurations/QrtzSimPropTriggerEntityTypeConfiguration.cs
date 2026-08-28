using Admin.Quartz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Admin.EntityTypeConfigurations
{
    public class QrtzSimPropTriggerEntityTypeConfiguration : IEntityTypeConfiguration<QrtzSimPropTrigger>
    {
        public void Configure(EntityTypeBuilder<QrtzSimPropTrigger> builder)
        {
            builder.ToTable("QrtzSimPropTriggers", AdminConsts.DbSchema, e => e.HasComment("Quartz QrtzSimPropTriggers"));

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.Property(x => x.StrProp1).HasMaxLength(500);
            builder.Property(x => x.StrProp2).HasMaxLength(500);
            builder.Property(x => x.StrProp3).HasMaxLength(500);
            builder.Property(x => x.IntProp1);
            builder.Property(x => x.IntProp2);
            builder.Property(x => x.LongProp1);
            builder.Property(x => x.LongProp2);
            builder.Property(x => x.DecProp1).HasPrecision(18, 6);
            builder.Property(x => x.DecProp2).HasPrecision(18, 6);
            builder.Property(x => x.BoolProp1);
            builder.Property(x => x.BoolProp2);

            builder.HasIndex(x => x.TriggerId)
                .IsUnique();

            builder.ConfigureFullAuditedAggregateRoot();
            builder.ConfigureByConvention();
        }
    }
}
