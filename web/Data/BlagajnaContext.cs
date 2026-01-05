using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using web.Models;

namespace web.Data
{

    public class BlagajnaContext : IdentityDbContext<ApplicationUser>
    {
        public BlagajnaContext(DbContextOptions<BlagajnaContext> options) : base(options)
        {
        }

        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Saved> SavedMoney { get; set;}
        public DbSet<Investment> Investments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Budget>().ToTable("Budget");
            modelBuilder.Entity<Category>().ToTable("Catrgory");
            modelBuilder.Entity<Transaction>().ToTable("Transaction");
            modelBuilder.Entity<Income>().ToTable("Income");
            modelBuilder.Entity<Saved>().ToTable("Saved");
            modelBuilder.Entity<Investment>().ToTable("Investment");

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
    }
}