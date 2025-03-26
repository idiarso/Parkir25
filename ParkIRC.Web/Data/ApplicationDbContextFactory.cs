using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ParkIRC.Web.Data
{
    public class ApplicationDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ApplicationDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Host=localhost;Database=parkirc;Username=postgres;Password=1q2w3e4r5t;Pooling=true";
            }
            optionsBuilder.UseNpgsql(connectionString);
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}