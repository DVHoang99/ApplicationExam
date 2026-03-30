using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAppExam.Domain;

namespace WebAppExam.Infrastructure.Persistence.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("customers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Ulid.Parse(v)
                )
                .HasMaxLength(40);

            builder.Property(x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            builder.Property(x => x.DeletedAt);

        }
    }
}
