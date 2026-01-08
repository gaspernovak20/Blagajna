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
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomMember> RoomMembers { get; set; }
        public DbSet<RoomExpense> RoomExpenses { get; set; }
        public DbSet<RoomExpenseParticipant> RoomExpenseParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Budget>().ToTable("Budget");
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Transaction>().ToTable("Transaction");
            modelBuilder.Entity<Income>().ToTable("Income");
            modelBuilder.Entity<Saved>().ToTable("Saved");
            modelBuilder.Entity<Investment>().ToTable("Investment");
            modelBuilder.Entity<Room>().HasIndex(r => r.Code).IsUnique();
            modelBuilder.Entity<RoomExpenseParticipant>().HasIndex(p => new { p.RoomExpenseId, p.UserId }).IsUnique();
            modelBuilder.Entity<RoomExpenseParticipant>().HasOne(p => p.RoomExpense).WithMany(e => e.Participants).HasForeignKey(p => p.RoomExpenseId).OnDelete(DeleteBehavior.NoAction);

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
    }
}