using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using WaitTimeTesting.Models;
using WaitTimeTesting.Options;

// For testing
// Junk

namespace WaitTimeTesting.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();

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

            // Configure Order for Orders table
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(o => o.Uid);
                entity.Property(o => o.CustomerId);
                entity.Property(o => o.PlacedAt).IsRequired();
                entity.Property(o => o.CompletedAt).IsRequired(false);
                entity.Property(o => o.ItemIds).IsRequired().HasMaxLength(100);
                entity.Property(o => o.Status).IsRequired().HasDefaultValue((byte)0);
                entity.Property(o => o.EstimatedWaitTime).HasColumnType("float");
                entity.Property(o => o.PlaceInQueue).IsRequired();
                entity.Property(o => o.TotalItemsAheadAtPlacement).HasColumnType("float");
                entity.Property(o => o.ItemsAheadAtPlacement)
                      .HasColumnType("nvarchar(MAX)")
                      .HasConversion(floatArrayToJsonConverter)
                      .Metadata.SetValueComparer(floatArrayComparer);
                entity.Property(o => o.ActualWaitMinutes).IsRequired(false);
                entity.Property(o => o.PredictionError).IsRequired(false);
            });
        }
    }
}