using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAppExam.Domain;

namespace WebAppExam.Infra.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasMaxLength(50)
                .ValueGeneratedNever();

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.DeletedAt);

            // CustomerId is a Guid; configure as required UUID column (no string length)
            builder.Property(p => p.CustomerId)
                .IsRequired();

            builder.Property(p => p.Status);

            builder.Property(p => p.UpdatedAt);

            // PaymentId is a Guid
            builder.Property(p => p.PaymentId)
                .IsRequired();

            builder.HasOne(o => o.PaymentDetail)
              .WithOne(p => p.Order)
              .HasForeignKey<Order>(o => o.PaymentId)
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Customer)
              .WithMany(p => p.Orders)
              .HasForeignKey(o => o.CustomerId)
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
