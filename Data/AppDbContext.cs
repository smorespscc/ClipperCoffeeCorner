using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using WaitTimeTesting.Models;
using WaitTimeTesting.Options;

namespace WaitTimeTesting.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> ActiveOrders => Set<Order>();
        public DbSet<Order> CompletedPendingTraining => Set<Order>();
        public DbSet<Order> TrainedOrders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var floatArrayToJsonConverter = new ValueConverter<float[], string>(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<float[]>(v, JsonSerializerOptions.Default) ?? new float[Constants.MaxMenuId]
            );

            var floatArrayComparer = new ValueComparer<float[]>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (hash, x) => HashCode.Combine(hash, x.GetHashCode())),
                v => v.ToArray()
            );

            // Configure Order for ActiveOrders table
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("ActiveOrders");
                entity.HasKey(o => o.Uid);
                entity.Property(o => o.Uid).ValueGeneratedOnAdd();
                entity.Property(o => o.ItemIds).IsRequired().HasMaxLength(100);
                entity.Property(o => o.PhoneNumber).HasMaxLength(20);
                entity.Property(o => o.EstimatedWaitTime).HasColumnType("float");
                entity.Property(o => o.TotalItemsAheadAtPlacement).HasColumnType("float");

                entity.Property(o => o.ItemsAheadAtPlacement)
                      .HasColumnType("nvarchar(255)")
                      .HasConversion(floatArrayToJsonConverter)
                      .Metadata.SetValueComparer(floatArrayComparer);
            });

            // Configure SAME Order for CompletedPendingTraining table
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("CompletedPendingTraining");
                entity.HasKey(o => o.Uid);
                entity.Property(o => o.EstimatedWaitTime).IsRequired();
                entity.Property(o => o.TotalItemsAheadAtPlacement).IsRequired();
                entity.Property(o => o.CompletedAt).IsRequired();

                entity.Property(o => o.ItemsAheadAtPlacement)
                      .IsRequired()
                      .HasColumnType("nvarchar(255)")
                      .HasConversion(floatArrayToJsonConverter)
                      .Metadata.SetValueComparer(floatArrayComparer);
            });

            // Optional: TrainedOrders table (if you want to persist it)
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("TrainedOrders");
                entity.HasKey(o => o.Uid);
                entity.Property(o => o.ItemsAheadAtPlacement)
                      .HasColumnType("nvarchar(255)")
                      .HasConversion(floatArrayToJsonConverter)
                      .Metadata.SetValueComparer(floatArrayComparer);
            });
        }
    }
}