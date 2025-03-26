using System;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Data.Models;

namespace ParkIRC.Data.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ParkingSpace> ParkingSpaces { get; set; }
        public DbSet<CameraConfig> CameraConfigs { get; set; }
        public DbSet<EntryGate> EntryGates { get; set; }
        public DbSet<OfflineEntry> OfflineEntries { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Rate> Rates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParkingSpace>()
                .HasOne(ps => ps.Vehicle)
                .WithOne(v => v.ParkingSpace)
                .HasForeignKey<ParkingSpace>(ps => ps.VehicleId);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.ParkingSpace)
                .WithOne(ps => ps.Vehicle)
                .HasForeignKey<Vehicle>(v => v.ParkingSpaceId);
        }
    }
}
