using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Models;
using ParkIRC.Web.Models;
using ParkIRC;

namespace ParkIRC.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ParkingSpace> ParkingSpaces { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<ParkingTransaction> ParkingTransactions { get; set; } = null!;
        public DbSet<Shift> Shifts { get; set; } = null!;
        public DbSet<Operator> Operators { get; set; } = null!;
        public DbSet<ParkingTicket> ParkingTickets { get; set; } = null!;
        public DbSet<Journal> Journals { get; set; } = null!;
        public DbSet<ParkingRateConfiguration> ParkingRates { get; set; } = null!;
        public DbSet<CameraSettings> CameraSettings { get; set; } = null!;
        public DbSet<EntryGate> EntryGates { get; set; } = null!;
        public DbSet<SiteSettings> SiteSettings { get; set; } = null!;
        public DbSet<PrinterConfig> PrinterConfigs { get; set; } = null!;
        public DbSet<VehicleEntry> VehicleEntries { get; set; }
        public DbSet<VehicleExit> VehicleExits { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ParkingSpace>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SpaceNumber).IsRequired();
                entity.Property(e => e.SpaceType).IsRequired();
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.CurrentVehicle)
                    .WithOne(v => v.ParkingSpace)
                    .HasForeignKey<ParkingSpace>(p => p.CurrentVehicleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VehicleNumber).IsRequired();
                entity.Property(e => e.VehicleType).IsRequired();
                entity.HasOne(e => e.Shift)
                    .WithMany(s => s.Vehicles)
                    .HasForeignKey(e => e.ShiftId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<ParkingTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionNumber).IsRequired();
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.ParkingTransactions)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ParkingSpace)
                    .WithMany(p => p.ParkingTransactions)
                    .HasForeignKey(e => e.ParkingSpaceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Shift>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ShiftName).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
            });

            builder.Entity<Operator>(entity =>
            {
                entity.Property(e => e.FullName).IsRequired();
                entity.HasMany(e => e.Shifts)
                    .WithMany(s => s.Operators)
                    .UsingEntity(j => j.ToTable("OperatorShifts"));
            });

            builder.Entity<ParkingTicket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TicketNumber).IsRequired();
                entity.Property(e => e.BarcodeData).IsRequired();
                entity.HasOne(e => e.Vehicle)
                    .WithMany()
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.IssuedByOperator)
                    .WithMany()
                    .HasForeignKey(e => e.OperatorId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Shift)
                    .WithMany()
                    .HasForeignKey(e => e.ShiftId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Journal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.OperatorId).IsRequired();
                
                entity.HasOne(e => e.Operator)
                      .WithMany()
                      .HasForeignKey(e => e.OperatorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ParkingRateConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VehicleType).IsRequired();
                entity.Property(e => e.BaseRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DailyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.WeeklyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MonthlyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PenaltyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.EffectiveFrom).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.LastModifiedBy).IsRequired();
            });

            builder.Entity<CameraSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProfileName).IsRequired();
                entity.Property(e => e.LightingCondition).HasDefaultValue("Normal");
            });

            builder.Entity<EntryGate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(255);
                entity.Property(e => e.IpAddress).HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasOne(g => g.Camera)
                    .WithMany()
                    .HasForeignKey(g => g.CameraId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(g => g.Printer)
                    .WithMany()
                    .HasForeignKey(g => g.PrinterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SiteSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SiteName).IsRequired();
                entity.Property(e => e.ThemeColor).HasDefaultValue("#007bff");
                entity.Property(e => e.ShowLogo).HasDefaultValue(true);
                entity.Property(e => e.LastUpdated).IsRequired();
                entity.Property(e => e.UpdatedBy).IsRequired();
            });

            builder.Entity<PrinterConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Port).IsRequired();
                entity.Property(e => e.ConnectionType).HasDefaultValue("Serial");
                entity.Property(e => e.Status).HasDefaultValue("Offline");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.LastChecked).IsRequired();
            });

            builder.Entity<VehicleEntry>(entity =>
            {
                entity.HasOne(ve => ve.Vehicle)
                    .WithMany(v => v.VehicleEntries)
                    .HasForeignKey(ve => ve.VehicleId);

                entity.HasOne(ve => ve.ParkingSpace)
                    .WithMany(ps => ps.VehicleEntries)
                    .HasForeignKey(ve => ve.ParkingSpaceId);
            });

            builder.Entity<VehicleExit>(entity =>
            {
                entity.HasOne(vx => vx.Vehicle)
                    .WithMany(v => v.VehicleExits)
                    .HasForeignKey(vx => vx.VehicleId);

                entity.HasOne(vx => vx.ParkingSpace)
                    .WithMany(ps => ps.VehicleExits)
                    .HasForeignKey(vx => vx.ParkingSpaceId);

                entity.HasOne(vx => vx.Transaction)
                    .WithMany()
                    .HasForeignKey(vx => vx.TransactionId);
            });

            builder.Entity<Vehicle>(entity =>
            {
                entity.HasIndex(v => v.PlateNumber)
                    .IsUnique();
            });

            builder.Entity<ParkingSpace>(entity =>
            {
                entity.HasIndex(ps => ps.Name)
                    .IsUnique();
            });

            // Configure operator mapping
            builder.Entity<Operator>()
                .HasMany(o => o.Transactions)
                .WithOne(t => t.Operator)
                .HasForeignKey(t => t.OperatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            var adminUser = new ApplicationUser
            {
                Id = "1",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                FirstName = "Admin",
                LastName = "User",
                IsOperator = true
            };

            var adminOperator = new Operator
            {
                Id = adminUser.Id,
                UserName = adminUser.UserName,
                NormalizedUserName = adminUser.NormalizedUserName,
                Name = $"{adminUser.FirstName} {adminUser.LastName}",
                FullName = $"{adminUser.FirstName} {adminUser.LastName}",
                Email = adminUser.Email,
                NormalizedEmail = adminUser.NormalizedEmail,
                IsActive = true,
                JoinDate = DateTime.UtcNow,
                SecurityStamp = adminUser.SecurityStamp,
                EmailConfirmed = true
            };

            builder.Entity<ApplicationUser>().HasData(adminUser);
            builder.Entity<Operator>().HasData(adminOperator);

            // Seed system admin
            builder.Entity<Operator>()
                .HasData(new Operator
                {
                    Id = "2",
                    UserName = "systemadmin",
                    NormalizedUserName = "SYSTEMADMIN",
                    Name = "System Admin",
                    FullName = "System Administrator",
                    Email = "sysadmin@example.com",
                    NormalizedEmail = "SYSADMIN@EXAMPLE.COM",
                    IsActive = true,
                    JoinDate = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    EmailConfirmed = true
                });
        }
    }
}