using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Linq.Expressions;

namespace ParkIRC.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyAllConfigurations(this ModelBuilder modelBuilder)
        {
            var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
                .ToList();

            foreach (var type in typesToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.ApplyConfiguration(configurationInstance);
            }
        }

        public static void ApplyGlobalFilters(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                    var falseConstant = Expression.Constant(false);
                    var filter = Expression.Lambda(Expression.Equal(property, falseConstant), parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }

                if (typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(IMultiTenant.TenantId));
                    var tenantId = Expression.Constant(GetCurrentTenantId());
                    var filter = Expression.Lambda(Expression.Equal(property, tenantId), parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }
        }

        public static void ConfigureDeleteBehavior(this ModelBuilder modelBuilder, DeleteBehavior deleteBehavior)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = deleteBehavior;
            }
        }

        public static void ConfigureStringProperties(this ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(string)))
            {
                if (property.GetMaxLength() == null)
                    property.SetMaxLength(256);
            }
        }

        public static void ConfigureDateTimeProperties(this ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
            {
                property.SetColumnType("datetime2");
            }
        }

        public static void ConfigureDecimalProperties(this ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                if (!property.GetColumnType().Contains("decimal"))
                    property.SetColumnType("decimal(18,2)");
            }
        }

        public static void ConfigureAuditProperties(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>("CreatedAt")
                        .IsRequired();

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<string>("CreatedBy")
                        .HasMaxLength(256);

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime?>("UpdatedAt");

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<string>("UpdatedBy")
                        .HasMaxLength(256);
                }
            }
        }

        public static void ConfigureConcurrencyTokens(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IConcurrencyAware).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<byte[]>("RowVersion")
                        .IsRowVersion();
                }
            }
        }

        public static void ConfigureIndexes(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.GetProperties()
                    .Where(p => p.Name.EndsWith("Id") || p.Name.EndsWith("Code"))
                    .ToList();

                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasIndex(property.Name);
                }
            }
        }

        private static string GetCurrentTenantId()
        {
            // TODO: Implement tenant resolution logic
            return "default";
        }
    }

    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }

    public interface IMultiTenant
    {
        string TenantId { get; set; }
    }

    public interface IAuditable
    {
        DateTime CreatedAt { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        string UpdatedBy { get; set; }
    }

    public interface IConcurrencyAware
    {
        byte[] RowVersion { get; set; }
    }
} 