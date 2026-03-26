using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Infrastructure.Persistence.Configurations;

public class MonthlyRevenueConfiguration : IEntityTypeConfiguration<MonthlyRevenue>
{
    public void Configure(EntityTypeBuilder<MonthlyRevenue> builder)
    {
        builder.ToTable("MonthlyRevenues");

        builder.HasKey(x => x.MonthYear);

        builder.Property(x => x.MonthYear)
               .HasMaxLength(10)
               .IsRequired();

        builder.Property(x => x.TotalRevenue)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.TotalOrders)
               .IsRequired();
    }
}

