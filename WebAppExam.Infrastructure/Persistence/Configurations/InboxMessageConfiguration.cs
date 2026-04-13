using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Infrastructure.Persistence.Configurations
{
    public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
    {
        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.ToTable("inbox_messages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Ulid.Parse(v)
                )
                .HasMaxLength(40);

            builder.Property(x => x.MessageId)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Content)
                .HasColumnType("text");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Error)
                .HasColumnType("text");

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.UpdatedAt)
                .HasColumnType("timestamp with time zone");

            builder.HasIndex(x => x.MessageId)
                .IsUnique()
                .HasDatabaseName("idx_inbox_messages_message_id");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("idx_inbox_messages_status");
        }
    }
}
