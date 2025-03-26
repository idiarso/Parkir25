using AdminCreator.Models;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using System;
using System.Threading.Tasks;

// Simple console app to create an admin user in the ParkIRC database

// Connection string
string connectionString = "Host=localhost;Database=parkirc;Username=postgres;Password=1q2w3e4r5t";

// Admin user details
string email = "admin@parking.com";
string password = "Admin@123";
string fullName = "System Administrator";

Console.WriteLine("Creating admin user...");

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
        ON CONFLICT (""NormalizedName"") DO NOTHING",
        conn))
    {
        roleCmd.Parameters.AddWithValue("id", roleId);
        roleCmd.Parameters.AddWithValue("stamp", Guid.NewGuid().ToString());
        await roleCmd.ExecuteNonQueryAsync();
    }
    
    // Get the Admin role ID if it already exists
    using (var getRoleCmd = new NpgsqlCommand(
        @"SELECT ""Id"" FROM ""AspNetRoles"" WHERE ""NormalizedName"" = 'ADMIN'",
        conn))
    {
        var result = await getRoleCmd.ExecuteScalarAsync();
        if (result != null)
        {
            roleId = result.ToString();
            Console.WriteLine($"Using existing Admin role with ID: {roleId}");
        }
        else
        {
            Console.WriteLine($"Created new Admin role with ID: {roleId}");
        }
    }
    
    // Delete existing admin user if exists
    using (var deleteCmd = new NpgsqlCommand(
        @"DELETE FROM ""AspNetUsers"" WHERE ""NormalizedEmail"" = @email",
        conn))
    {
        deleteCmd.Parameters.AddWithValue("email", email.ToUpper());
        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();
        if (rowsAffected > 0)
        {
            Console.WriteLine("Deleted existing admin user");
        }
    }
    
    // Create admin user with all required fields
    using (var userCmd = new NpgsqlCommand(
        @"INSERT INTO ""AspNetUsers"" (
            ""Id"", ""UserName"", ""NormalizedUserName"", ""Email"", ""NormalizedEmail"",
            ""EmailConfirmed"", ""PasswordHash"", ""SecurityStamp"", ""ConcurrencyStamp"",
            ""PhoneNumberConfirmed"", ""TwoFactorEnabled"", ""LockoutEnabled"", ""AccessFailedCount"",
            ""FirstName"", ""LastName"", ""Name"", ""FullName"", ""IsOnDuty"", ""IsActive"", ""IsOperator"", 
            ""JoinDate"", ""CreatedAt"")
        VALUES (
            @id, @email, @normEmail, @email, @normEmail,
            @emailConf, @pwHash, @secStamp, @concStamp,
            @phoneConf, @twoFactor, @lockout, @failCount,
            @firstName, @lastName, @name, @fullName, @isOnDuty, @isActive, @isOperator,
            @joinDate, @createdAt)
        RETURNING ""Id""",
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
        userCmd.Parameters.AddWithValue("firstName", "System");
        userCmd.Parameters.AddWithValue("lastName", "Administrator");
        userCmd.Parameters.AddWithValue("name", "Administrator");
        userCmd.Parameters.AddWithValue("fullName", fullName);
        userCmd.Parameters.AddWithValue("isOnDuty", false);
        userCmd.Parameters.AddWithValue("isActive", true);
        userCmd.Parameters.AddWithValue("isOperator", true);
        userCmd.Parameters.AddWithValue("joinDate", DateTime.UtcNow);
        userCmd.Parameters.AddWithValue("createdAt", DateTime.UtcNow);
        
        var result = await userCmd.ExecuteScalarAsync();
        if (result != null)
        {
            userId = result.ToString();
            Console.WriteLine($"Created admin user with ID: {userId}");
        }
        else
        {
            Console.WriteLine("Failed to create admin user");
            return;
        }
    }
    
    // Add user to Admin role
    using (var userRoleCmd = new NpgsqlCommand(
        @"INSERT INTO ""AspNetUserRoles"" (""UserId"", ""RoleId"")
        VALUES (@userId, @roleId)
        ON CONFLICT (""UserId"", ""RoleId"") DO NOTHING",
        conn))
    {
        userRoleCmd.Parameters.AddWithValue("userId", userId);
        userRoleCmd.Parameters.AddWithValue("roleId", roleId);
        await userRoleCmd.ExecuteNonQueryAsync();
        Console.WriteLine("Added admin user to Admin role");
    }
    
    Console.WriteLine("Success! Admin user created with:");
    Console.WriteLine($"Email: {email}");
    Console.WriteLine($"Password: {password}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        Console.WriteLine(ex.InnerException.StackTrace);
    }
    
    Console.WriteLine(ex.StackTrace);
}
