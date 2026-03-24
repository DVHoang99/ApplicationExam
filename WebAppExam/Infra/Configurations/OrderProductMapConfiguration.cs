using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAppExam.Domain;

namespace WebAppExam.Infra.Configurations
{
    public class OrderProductMapConfiguration : IEntityTypeConfiguration<OrderProductMap>
    {
        public void Configure(EntityTypeBuilder<OrderProductMap> builder)
        {
            builder.ToTable("order_product_maps");
            
            builder.Ignore(p => p.Id);

            builder.HasKey(p => new { p.OrderId, p.ProductId});

            builder.Property(p => p.ProductId)
                .HasMaxLength(50);
            builder.Property(p => p.ProductId)
                .HasMaxLength(50);
            builder.Property(p => p.Quantity)
                .HasDefaultValue(0);
        }
    }
}
