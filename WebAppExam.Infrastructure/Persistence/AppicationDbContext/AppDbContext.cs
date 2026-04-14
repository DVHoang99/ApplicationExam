using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Infrastructure.Persistence.AppicationDbContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<MonthlyRevenue> MonthlyRevenues { get; private set; }
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public static string FUnaccent(string input) => throw new NotSupportedException();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDbFunction(typeof(AppDbContext).GetMethod(nameof(FUnaccent))!)
                    .HasName("f_unaccent");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
