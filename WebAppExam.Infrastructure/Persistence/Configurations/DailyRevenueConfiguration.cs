using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Infrastructure.Persistence.Configurations;

public class DailyRevenueConfiguration : IEntityTypeConfiguration<DailyRevenue>
{
    public void Configure(EntityTypeBuilder<DailyRevenue> builder)
    {
        builder.ToTable("daily_revenues");
        builder.Ignore(x => x.Id);
        builder.Ignore(x => x.CreatedAt);
        builder.Ignore(x => x.UpdatedAt);
        builder.Ignore(x => x.DeletedAt);

        builder.HasKey(x => x.Date);

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.TotalOrders)
            .IsRequired();

        builder.Property(x => x.TotalRevenue)
            .IsRequired();
    }
}
