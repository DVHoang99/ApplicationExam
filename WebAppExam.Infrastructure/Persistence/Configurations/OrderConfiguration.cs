using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAppExam.Domain;

namespace WebAppExam.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Ulid.Parse(v)
                )
                .HasMaxLength(40);
            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.CustomerId)
                .HasConversion(
                    v => v.ToString(),
                    v => Ulid.Parse(v)
                )
                .IsRequired()
                .HasMaxLength(40);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            builder.Property(x => x.DeletedAt);

            builder.Property(x => x.Address)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.CustomerName)
                .IsRequired();

            builder.Property(x => x.PhoneNumber)
                .IsRequired();
            builder.Property(x => x.Reason)
            .HasMaxLength(200);

            builder.HasIndex(x => x.CustomerId);
        }
    }
}
