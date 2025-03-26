using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParkIRC.Data;
using ParkIRC.Models;

// Create a standalone program to add an admin user
namespace CreateAdmin
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup dependency injection
            var services = new ServiceCollection();
            
            // Add database context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql("Host=localhost;Database=parkirc;Username=postgres;Password=1q2w3e4r5t"));
            
            // Add Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            
            var serviceProvider = services.BuildServiceProvider();
            
            using (var scope = serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                
                // Check if admin@parking.com exists
                var existingUser = await userManager.FindByEmailAsync("admin@parking.com");
                if (existingUser != null)
                {
                    // Delete existing user
                    Console.WriteLine("Deleting existing admin user...");
                    await userManager.DeleteAsync(existingUser);
                }
                
                // Create a new admin user
                Console.WriteLine("Creating admin user...");
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@parking.com",
                    Email = "admin@parking.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    JoinDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                    IsOperator = true
                };
                
                // Create admin user
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    Console.WriteLine("Admin user created successfully.");
                    
                    // Create Admin role if it doesn't exist
                    if (!await roleManager.RoleExistsAsync("Admin"))
                    {
                        Console.WriteLine("Creating Admin role...");
                        await roleManager.CreateAsync(new IdentityRole("Admin"));
                    }
                    
                    // Assign admin user to Admin role
                    Console.WriteLine("Assigning admin user to Admin role...");
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    
                    Console.WriteLine("Done!");
                }
                else
                {
                    Console.WriteLine("Failed to create admin user:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"- {error.Code}: {error.Description}");
                    }
                }
            }
        }
    }
} 