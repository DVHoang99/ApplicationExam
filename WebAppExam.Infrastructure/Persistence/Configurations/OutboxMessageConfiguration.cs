using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Infrastructure.Persistence.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Ulid.Parse(v)
                )
                .HasMaxLength(40);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Content)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.ProcessedOn)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.Error)
                .HasMaxLength(1024)
                .HasColumnType("text");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.MessageId)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("idx_outbox_messages_status");

            builder.HasIndex(x => x.CreatedAt)
                .HasDatabaseName("idx_outbox_messages_created_at");

            builder.HasIndex(x => new { x.Status, x.CreatedAt })
                .HasDatabaseName("idx_outbox_messages_status_created_at");

            builder.HasIndex(x => x.MessageId)
                .HasDatabaseName("idx_outbox_messages_message_id");
        }
    }
}
