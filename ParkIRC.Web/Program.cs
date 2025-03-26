using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParkIRC.Web.Data;
using ParkIRC.Web.Models;
using ParkIRC.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ParkIRC.Web.Extensions;
using ParkIRC.Data.Data;
using ParkIRC.Data.Services;

namespace ParkIRC.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            // Seed initial data
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    await SeedData.InitializeAsync(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(builder => 
                {
                    builder.Services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

                    builder.Services.AddScoped<IParkingService, ParkingService>();
                    builder.Services.AddScoped<IPrinterService, PrintService>();
                    builder.Services.AddScoped<ITicketService, TicketService>();
                    builder.Services.AddScoped<IStorageService, StorageService>();
                    builder.Services.AddScoped<ICameraService, CameraService>();
                    builder.Services.AddScoped<IConnectionStatusService, ConnectionStatusService>();
                    builder.Services.AddScoped<ISystemService, SystemService>();
                    builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
                    builder.Services.AddScoped<IShiftService, ShiftService>();
                });
    }
}
