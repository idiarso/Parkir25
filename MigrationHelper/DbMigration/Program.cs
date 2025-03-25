using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DbMigration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DB Migration Helper");
            
            // Build configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
                
            // Set up dependency injection
            var services = new ServiceCollection();
            
            // Add database context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsAssembly("DbMigration")));
                    
            // Build service provider
            var serviceProvider = services.BuildServiceProvider();
            
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    Console.WriteLine("Checking database connection...");
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    
                    // Test connection
                    if (dbContext.Database.CanConnect())
                    {
                        Console.WriteLine("Connection to database successful!");
                        
                        // Create migrations folder if it doesn't exist
                        Directory.CreateDirectory("Migrations");
                        
                        Console.WriteLine("Applying migrations...");
                        dbContext.Database.Migrate();
                        
                        Console.WriteLine("Migrations applied successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect to database. Check your connection string.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
