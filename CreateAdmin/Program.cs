using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Npgsql;

// Simple console app to create an admin user in the ParkIRC database
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Creating admin user...");
        
        // Connection string
        string connectionString = "Host=localhost;Database=parkirc;Username=postgres;Password=1q2w3e4r5t";
        
        // Admin user details
        string email = "admin@parking.com";
        string password = "Admin@123";
        string fullName = "System Administrator";
        
        try
        {
            // Create connection
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            
            // Check if user already exists
            using var checkCmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM \"AspNetUsers\" WHERE \"Email\" = @email", 
                conn);
            
            checkCmd.Parameters.AddWithValue("email", email);
            long count = (long)await checkCmd.ExecuteScalarAsync();
            
            if (count > 0)
            {
                Console.WriteLine($"User {email} already exists!");
                return;
            }
            
            // Generate password hash
            var hasher = new PasswordHasher<object>();
            var hash = hasher.HashPassword(null, password);
            
            // Generate a new GUID for user ID
            string userId = Guid.NewGuid().ToString();
            
            // Insert user
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO ""AspNetUsers"" (
                    ""Id"", ""UserName"", ""NormalizedUserName"", ""Email"", ""NormalizedEmail"", 
                    ""EmailConfirmed"", ""PasswordHash"", ""SecurityStamp"", ""ConcurrencyStamp"",
                    ""PhoneNumberConfirmed"", ""TwoFactorEnabled"", ""LockoutEnabled"", ""AccessFailedCount"",
                    ""Name"", ""FullName"", ""IsOnDuty"") 
                VALUES (
                    @id, @email, @normalizedEmail, @email, @normalizedEmail, 
                    true, @passwordHash, @securityStamp, @concurrencyStamp,
                    false, false, true, 0,
                    @fullName, @fullName, false)",
                conn);
            
            cmd.Parameters.AddWithValue("id", userId);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("normalizedEmail", email.ToUpperInvariant());
            cmd.Parameters.AddWithValue("passwordHash", hash);
            cmd.Parameters.AddWithValue("securityStamp", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("concurrencyStamp", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("fullName", fullName);
            
            await cmd.ExecuteNonQueryAsync();
            
            // Check if Admin role exists
            using var checkRoleCmd = new NpgsqlCommand(
                "SELECT \"Id\" FROM \"AspNetRoles\" WHERE \"Name\" = 'Admin'", 
                conn);
            
            var roleId = await checkRoleCmd.ExecuteScalarAsync() as string;
            
            if (roleId == null)
            {
                // Create Admin role
                roleId = Guid.NewGuid().ToString();
                using var createRoleCmd = new NpgsqlCommand(
                    @"INSERT INTO ""AspNetRoles"" (""Id"", ""Name"", ""NormalizedName"", ""ConcurrencyStamp"")
                    VALUES (@id, 'Admin', 'ADMIN', @concurrencyStamp)",
                    conn);
                
                createRoleCmd.Parameters.AddWithValue("id", roleId);
                createRoleCmd.Parameters.AddWithValue("concurrencyStamp", Guid.NewGuid().ToString());
                await createRoleCmd.ExecuteNonQueryAsync();
            }
            
            // Assign user to Admin role
            using var userRoleCmd = new NpgsqlCommand(
                @"INSERT INTO ""AspNetUserRoles"" (""UserId"", ""RoleId"")
                VALUES (@userId, @roleId)",
                conn);
            
            userRoleCmd.Parameters.AddWithValue("userId", userId);
            userRoleCmd.Parameters.AddWithValue("roleId", roleId);
            await userRoleCmd.ExecuteNonQueryAsync();
            
            Console.WriteLine($"Admin user '{email}' created successfully with password '{password}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
