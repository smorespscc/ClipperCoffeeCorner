using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using WaitTimeTesting.Data.Entities;
using WaitTimeTesting.Models;
using WaitTimeTesting.Options;

namespace WaitTimeTesting.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ActiveOrderEntity> ActiveOrders => Set<ActiveOrderEntity>();
        public DbSet<CompletedPendingTrainingOrderEntity> CompletedPendingTraining => Set<CompletedPendingTrainingOrderEntity>();
        public DbSet<TrainedOrderEntity> TrainedOrders => Set<TrainedOrderEntity>();

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
            modelBuilder.Entity<ActiveOrderEntity>(entity =>
            {
                entity.ToTable("ActiveOrders");
                entity.HasKey(o => o.Uid);
                entity.Property(o => o.PlacedAt).IsRequired();
                entity.Property(o => o.CompletedAt).IsRequired(false);
                entity.Property(o => o.ItemIds).IsRequired().HasMaxLength(100);
                entity.Property(o => o.Status).IsRequired().HasDefaultValue((byte)0);
                entity.Property(o => o.PhoneNumber).HasMaxLength(20);
                entity.Property(o => o.NotificationPref).IsRequired().HasDefaultValue((byte)0);
                entity.Property(o => o.EstimatedWaitTime).HasColumnType("float");
                entity.Property(o => o.PlaceInQueue).IsRequired();
                entity.Property(o => o.TotalItemsAheadAtPlacement).HasColumnType("float");
                entity.Property(o => o.ItemsAheadAtPlacement)
                      .HasColumnType("nvarchar(MAX)")
                      .HasConversion(floatArrayToJsonConverter)
                      .Metadata.SetValueComparer(floatArrayComparer);
                entity.Property(o => o.ActualWaitMinutes).IsRequired(false);
                entity.Property(o => o.PredictionError).IsRequired(false);
                entity.HasIndex(o => o.PlacedAt).HasDatabaseName("IX_ActiveOrders_PlacedAt");
            });

            // Configure Order for CompletedPendingTraining table
            modelBuilder.Entity<CompletedPendingTrainingOrderEntity>(entity =>
            {
                entity.ToTable("CompletedPendingTraining");
                entity.HasKey(o => o.Uid);
                entity.Property(o => o.PlacedAt).IsRequired();
                entity.Property(o => o.ItemIds).IsRequired().HasMaxLength(100);
                entity.Property(o => o.PhoneNumber).HasMaxLength(20);
                entity.Property(o => o.NotificationPref).IsRequired().HasDefaultValue((byte)0);
                entity.Property(o => o.Status).IsRequired().HasDefaultValue((byte)0);
                entity.Property(o => o.PlaceInQueue).IsRequired();
                entity.Property(o => o.EstimatedWaitTime).HasColumnType("float").IsRequired();
                entity.Property(o => o.ItemsAheadAtPlacement).HasColumnType("nvarchar(max)").IsRequired().HasConversion(floatArrayToJsonConverter).Metadata.SetValueComparer(floatArrayComparer);
                entity.Property(o => o.TotalItemsAheadAtPlacement).HasColumnType("float").IsRequired();
                entity.Property(o => o.CompletedAt).IsRequired();
                entity.Property(o => o.ActualWaitMinutes).HasColumnType("float").IsRequired();
                entity.Property(o => o.PredictionError).HasColumnType("float").IsRequired();
                entity.HasIndex(o => o.PlacedAt).HasDatabaseName("IX_CompletedPendingTraining_PlacedAt");
            });

            // Configure Order for TrainedOrders table
            modelBuilder.Entity<TrainedOrderEntity>(entity =>
            {
                entity.ToTable("TrainedOrders");
                entity.HasKey(o => o.Uid);
                entity.Property(o => o.PlacedAt).IsRequired();
                entity.Property(o => o.ItemIds).IsRequired().HasMaxLength(100);
                entity.Property(o => o.PhoneNumber).HasMaxLength(20);
                entity.Property(o => o.NotificationPref).IsRequired().HasDefaultValue((byte)0);
                entity.Property(o => o.Status).IsRequired().HasDefaultValue((byte)0);
                entity.Property(o => o.PlaceInQueue).IsRequired();
                entity.Property(o => o.EstimatedWaitTime).HasColumnType("float").IsRequired();
                entity.Property(o => o.ItemsAheadAtPlacement).HasColumnType("nvarchar(max)").IsRequired().HasConversion(floatArrayToJsonConverter).Metadata.SetValueComparer(floatArrayComparer);
                entity.Property(o => o.TotalItemsAheadAtPlacement).HasColumnType("float").IsRequired();
                entity.Property(o => o.CompletedAt).IsRequired();
                entity.Property(o => o.ActualWaitMinutes).HasColumnType("float").IsRequired();
                entity.Property(o => o.PredictionError).HasColumnType("float").IsRequired();
                entity.HasIndex(o => o.PlacedAt).HasDatabaseName("IX_TrainedOrders_PlacedAt");
            });
        }
    }
}