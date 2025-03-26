using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParkIRC.Web.Data;
using ParkIRC.Web.Models;
using ParkIRC.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.IO;
using ParkIRC.Web.Extensions;
using ParkIRC.Data.Services;
using ParkIRC.Web.Extensions;

namespace ParkIRC.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            // Add Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Add services
            services.AddScoped<IParkingService, ParkingService>();
            services.AddScoped<IPrinterService, PrintService>();
            services.AddScoped<ICameraService, CameraService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IStorageService, StorageService>();
            services.AddScoped<IConnectionStatusService, ConnectionStatusService>();
            services.AddScoped<IEntryValidationService, EntryValidationService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            services.AddScoped<ISiteSettingsService, SiteSettingsService>();
            services.AddScoped<IOfflineDataService, OfflineDataService>();
            services.AddScoped<IShiftService, ShiftService>();
            services.AddScoped<IRateService, RateService>();

            // Add SignalR
            services.AddSignalR();

            // Add MVC
            services.AddControllersWithViews();
            services.AddRazorPages();

            // Configure file provider for static files
            services.Configure<StaticFileOptions>(options =>
            {
                options.FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
                options.RequestPath = "/wwwroot";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapHub<ParkingHub>("/parkingHub");
            });
        }
    }
}