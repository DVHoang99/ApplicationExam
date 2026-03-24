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

            builder.Property(p => p.CustomerId)
                .HasMaxLength(50);

            builder.Property(p => p.Status);

            builder.Property(p => p.UpdatedAt);

            builder.Property(p => p.PaymentId)
                .HasMaxLength(50);

            builder.HasOne(o => o.PaymentDetail)
              .WithOne(p => p.Order)
              .HasForeignKey<Order>(o => o.PaymentId)
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
