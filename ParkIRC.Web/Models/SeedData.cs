using Microsoft.EntityFrameworkCore;
using ParkIRC.Web.Data;
using ParkIRC.Web.Models;
using System.Threading.Tasks;

namespace ParkIRC.Web.Models
{
    public class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await context.Database.EnsureCreatedAsync();

            // Seed camera configurations
            if (!await context.Cameras.AnyAsync())
            {
                await context.Cameras.AddRangeAsync(new[]
                {
                    new CameraConfig
                    {
                        Name = "Entry Camera 1",
                        Url = "http://camera1.local/video",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new CameraConfig
                    {
                        Name = "Exit Camera 1",
                        Url = "http://camera2.local/video",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                });
            }

            // Seed printer configurations
            if (!await context.Printers.AnyAsync())
            {
                await context.Printers.AddRangeAsync(new[]
                {
                    new PrinterConfig
                    {
                        Name = "Entry Printer",
                        PrinterName = "EntryPrinter1",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new PrinterConfig
                    {
                        Name = "Exit Printer",
                        PrinterName = "ExitPrinter1",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                });
            }

            // Seed entry gates
            if (!await context.EntryGates.AnyAsync())
            {
                await context.EntryGates.AddRangeAsync(new[]
                {
                    new EntryGate
                    {
                        Name = "Main Entry",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                });
            }

            // Seed exit gates
            if (!await context.ExitGates.AnyAsync())
            {
                await context.ExitGates.AddRangeAsync(new[]
                {
                    new ExitGate
                    {
                        Name = "Main Exit",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                });
            }

            // Seed parking rates
            if (!await context.ParkingRates.AnyAsync())
            {
                await context.ParkingRates.AddRangeAsync(new[]
                {
                    new ParkingRate
                    {
                        VehicleType = "Car",
                        BaseRate = 2000,
                        RatePerHour = 1000,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new ParkingRate
                    {
                        VehicleType = "Motorcycle",
                        BaseRate = 1000,
                        RatePerHour = 500,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                });
            }

            // Seed site settings
            if (!await context.SiteSettings.AnyAsync())
            {
                await context.SiteSettings.AddAsync(new SiteSettings
                {
                    SiteName = "Parking Management System",
                    Theme = "light",
                    CurrencySymbol = "$",
                    TimeFormat = "HH:mm",
                    DateFormat = "yyyy-MM-dd",
                    CreatedAt = DateTime.Now
                });
            }

            await context.SaveChangesAsync();
        }
    }
}