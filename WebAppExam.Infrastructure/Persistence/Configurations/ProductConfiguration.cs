using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAppExam.Domain;

namespace WebAppExam.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => Ulid.Parse(v)
            )
            .HasMaxLength(40);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Price)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.DeletedAt);
        builder.Property(x => x.CorrelationId)
        .HasMaxLength(40);

        builder.Property(x => x.ProductStatus)
            .IsRequired();

        builder.Property(x => x.WareHouseId)
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(x => x.Name);
    }
}
