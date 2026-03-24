using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAppExam.Domain;

namespace WebAppExam.Infra.Configurations
{
    public class PaymentDetailConfiguration : IEntityTypeConfiguration<PaymentDetail>
    {
        public void Configure(EntityTypeBuilder<PaymentDetail> builder)
        {
            builder.ToTable("payment_details");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasMaxLength(50)
                .ValueGeneratedNever();

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.DeletedAt);

            builder.Property(p => p.UpdatedAt);

            builder.Property(p => p.OrderId)
                .HasMaxLength(50);

            builder.HasOne(o => o.Order)
          .WithOne(p => p.PaymentDetail)
          .HasForeignKey<PaymentDetail>(o => o.OrderId)
          .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
