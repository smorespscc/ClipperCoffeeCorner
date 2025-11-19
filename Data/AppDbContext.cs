using ClipperCoffeeCorner.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ClipperCoffeeCorner.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Password> Passwords => Set<Password>();
        public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<OptionGroup> OptionGroups => Set<OptionGroup>();
        public DbSet<OptionValue> OptionValues => Set<OptionValue>();
        public DbSet<MenuItemOptionGroup> MenuItemOptionGroups => Set<MenuItemOptionGroup>();
        public DbSet<Combination> Combinations => Set<Combination>();
        public DbSet<CombinationOption> CombinationOptions => Set<CombinationOption>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // composite keys
            modelBuilder.Entity<MenuItemOptionGroup>()
                .HasKey(m => new { m.MenuItemId, m.OptionGroupId });

            modelBuilder.Entity<CombinationOption>()
                .HasKey(c => new { c.CombinationId, c.OptionValueId });
        }
    }
}
