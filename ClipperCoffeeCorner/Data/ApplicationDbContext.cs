using Microsoft.EntityFrameworkCore;
using ClipperCoffeeCorner.Models;

namespace ClipperCoffeeCorner.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Combination> Combinations { get; set; } = null!;
        public DbSet<CombinationOption> CombinationOptions { get; set; } = null!;
        public DbSet<MenuCategory> MenuCategories { get; set; } = null!;
        public DbSet<MenuItem> MenuItems { get; set; } = null!;
        public DbSet<MenuItemOptionGroup> MenuItemOptionGroups { get; set; } = null!;
        public DbSet<OptionGroup> OptionGroups { get; set; } = null!;
        public DbSet<OptionValue> OptionValues { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Password> Passwords { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<Password>()
                .HasKey(p => p.UserId);

            modelBuilder.Entity<Password>()
                .HasOne<User>()
                .WithMany(u => u.Passwords)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MenuCategory>()
                .HasMany(c => c.MenuItems)
                .WithOne(i => i.MenuCategory)
                .HasForeignKey(i => i.MenuCategoryId);

            modelBuilder.Entity<MenuItem>()
                .HasMany(i => i.MenuItemOptionGroups)
                .WithOne(g => g.MenuItem)
                .HasForeignKey(g => g.MenuItemId);

            modelBuilder.Entity<OptionGroup>()
                .HasMany(g => g.OptionValues)
                .WithOne(v => v.OptionGroup)
                .HasForeignKey(v => v.OptionGroupId);

            modelBuilder.Entity<Combination>()
                .HasMany(c => c.CombinationOptions)
                .WithOne(o => o.Combination)
                .HasForeignKey(o => o.CombinationId);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId);
        }
    }
}