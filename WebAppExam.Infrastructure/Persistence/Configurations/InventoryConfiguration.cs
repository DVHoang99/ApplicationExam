using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Infrastructure.Persistence.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("inventories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => Ulid.Parse(v)
            )
            .HasMaxLength(40);

        builder.Property(x => x.ProductId)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => Ulid.Parse(v)
            )
            .HasMaxLength(40);

        builder.Property(x => x.Stock)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.DeletedAt);

        builder.HasIndex(x => x.ProductId).IsUnique();
    }
}
