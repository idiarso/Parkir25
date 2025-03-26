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
            
            // Generate password hash
            var hasher = new PasswordHasher<object>();
            var hash = hasher.HashPassword(null, password);
            
            // Generate a new GUID for user ID
            string userId = Guid.NewGuid().ToString();
            
            // Create an Admin role
            string roleId = Guid.NewGuid().ToString();
            using (var roleCmd = new NpgsqlCommand(
                @"INSERT INTO ""AspNetRoles"" (""Id"", ""Name"", ""NormalizedName"", ""ConcurrencyStamp"")
                VALUES (@id, 'Admin', 'ADMIN', @stamp)
                ON CONFLICT DO NOTHING",
                conn))
            {
                roleCmd.Parameters.AddWithValue("id", roleId);
                roleCmd.Parameters.AddWithValue("stamp", Guid.NewGuid().ToString());
                await roleCmd.ExecuteNonQueryAsync();
            }
            
            // Get the Admin role ID if it already exists
            using (var getRoleCmd = new NpgsqlCommand(
                @"SELECT ""Id"" FROM ""AspNetRoles"" WHERE ""Name"" = 'Admin'",
                conn))
            {
                var result = await getRoleCmd.ExecuteScalarAsync();
                if (result != null)
                {
                    roleId = result.ToString();
                }
            }
            
            // Create admin user with minimal required fields
            using (var userCmd = new NpgsqlCommand(
                @"INSERT INTO ""AspNetUsers"" (
                    ""Id"", ""UserName"", ""NormalizedUserName"", ""Email"", ""NormalizedEmail"",
                    ""EmailConfirmed"", ""PasswordHash"", ""SecurityStamp"", ""ConcurrencyStamp"",
                    ""PhoneNumberConfirmed"", ""TwoFactorEnabled"", ""LockoutEnabled"", ""AccessFailedCount"",
                    ""FullName"", ""Name"", ""IsOnDuty"", ""IsActive"")
                VALUES (
                    @id, @email, @normEmail, @email, @normEmail,
                    @emailConf, @pwHash, @secStamp, @concStamp,
                    @phoneConf, @twoFactor, @lockout, @failCount,
                    @fullName, @name, @isOnDuty, @isActive)
                ON CONFLICT (""Id"") DO NOTHING",
                conn))
            {
                userCmd.Parameters.AddWithValue("id", userId);
                userCmd.Parameters.AddWithValue("email", email);
                userCmd.Parameters.AddWithValue("normEmail", email.ToUpper());
                userCmd.Parameters.AddWithValue("emailConf", true);
                userCmd.Parameters.AddWithValue("pwHash", hash);
                userCmd.Parameters.AddWithValue("secStamp", Guid.NewGuid().ToString());
                userCmd.Parameters.AddWithValue("concStamp", Guid.NewGuid().ToString());
                userCmd.Parameters.AddWithValue("phoneConf", false);
                userCmd.Parameters.AddWithValue("twoFactor", false);
                userCmd.Parameters.AddWithValue("lockout", false);
                userCmd.Parameters.AddWithValue("failCount", 0);
                userCmd.Parameters.AddWithValue("fullName", fullName);
                userCmd.Parameters.AddWithValue("name", fullName);
                userCmd.Parameters.AddWithValue("isOnDuty", false);
                userCmd.Parameters.AddWithValue("isActive", true);
                
                await userCmd.ExecuteNonQueryAsync();
            }
            
            // Add user to Admin role
            using (var userRoleCmd = new NpgsqlCommand(
                @"INSERT INTO ""AspNetUserRoles"" (""UserId"", ""RoleId"")
                VALUES (@userId, @roleId)
                ON CONFLICT DO NOTHING",
                conn))
            {
                userRoleCmd.Parameters.AddWithValue("userId", userId);
                userRoleCmd.Parameters.AddWithValue("roleId", roleId);
                await userRoleCmd.ExecuteNonQueryAsync();
            }
            
            Console.WriteLine($"Admin user '{email}' created with password '{password}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
} 