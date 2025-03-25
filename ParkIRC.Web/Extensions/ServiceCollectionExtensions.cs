using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System;
using System.Linq;
using ParkIRC.Web.Models;

namespace ParkIRC.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add services from configuration
            services.Configure<SiteSettings>(configuration.GetSection("SiteSettings"));

            // Add services from assembly
            var assembly = Assembly.GetExecutingAssembly();
            var serviceTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"));

            foreach (var serviceType in serviceTypes)
            {
                var interfaceType = serviceType.GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"I{serviceType.Name}");

                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, serviceType);
                }
                else
                {
                    services.AddScoped(serviceType);
                }
            }

            return services;
        }

        public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var repositoryTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"));

            foreach (var repositoryType in repositoryTypes)
            {
                var interfaceType = repositoryType.GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"I{repositoryType.Name}");

                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, repositoryType);
                }
                else
                {
                    services.AddScoped(repositoryType);
                }
            }

            return services;
        }

        public static IServiceCollection AddApplicationValidators(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var validatorTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Validator"));

            foreach (var validatorType in validatorTypes)
            {
                var interfaceType = validatorType.GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"I{validatorType.Name}");

                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, validatorType);
                }
                else
                {
                    services.AddScoped(validatorType);
                }
            }

            return services;
        }

        public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Handler"));

            foreach (var handlerType in handlerTypes)
            {
                var interfaceType = handlerType.GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"I{handlerType.Name}");

                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, handlerType);
                }
                else
                {
                    services.AddScoped(handlerType);
                }
            }

            return services;
        }

        public static IServiceCollection AddApplicationMiddleware(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var middlewareTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Middleware"));

            foreach (var middlewareType in middlewareTypes)
            {
                services.AddScoped(middlewareType);
            }

            return services;
        }

        public static IServiceCollection AddApplicationFilters(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var filterTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Filter"));

            foreach (var filterType in filterTypes)
            {
                services.AddScoped(filterType);
            }

            return services;
        }

        public static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var optionTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Options"));

            foreach (var optionType in optionTypes)
            {
                var sectionName = optionType.Name.Replace("Options", "");
                var method = typeof(OptionsConfigurationServiceCollectionExtensions)
                    .GetMethod("Configure", new[] { typeof(IServiceCollection), typeof(IConfiguration) });

                if (method != null)
                {
                    var genericMethod = method.MakeGenericMethod(optionType);
                    genericMethod.Invoke(null, new object[] { services, configuration.GetSection(sectionName) });
                }
            }

            services.Configure<ParkIRC.Web.Models.SiteSettings>(configuration.GetSection("SiteSettings"));

            return services;
        }

        public static IServiceCollection AddApplicationAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JWT";
                options.DefaultChallengeScheme = "JWT";
            })
            .AddJwtBearer("JWT", options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(configuration["JWT:Key"]))
                };
            });

            return services;
        }

        public static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireManagerRole", policy => policy.RequireRole("Manager"));
                options.AddPolicy("RequireSupervisorRole", policy => policy.RequireRole("Supervisor"));
                options.AddPolicy("RequireOperatorRole", policy => policy.RequireRole("Operator"));
                options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Staff"));
            });

            return services;
        }

        public static IServiceCollection AddApplicationCors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });

                options.AddPolicy("AllowSpecific", builder =>
                {
                    builder.WithOrigins(configuration.GetSection("CORS:AllowedOrigins").Get<string[]>())
                           .WithMethods(configuration.GetSection("CORS:AllowedMethods").Get<string[]>())
                           .WithHeaders(configuration.GetSection("CORS:AllowedHeaders").Get<string[]>())
                           .AllowCredentials();
                });
            });

            return services;
        }

        public static IServiceCollection AddApplicationMvc(this IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.Add<Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.WriteIndented = true;
            });

            return services;
        }

        public static IServiceCollection AddApplicationSession(this IServiceCollection services)
        {
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            return services;
        }

        public static IServiceCollection AddApplicationAntiforgery(this IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "XSRF-TOKEN";
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            });

            return services;
        }

        public static IServiceCollection AddApplicationResponseCompression(this IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            });

            return services;
        }

        public static IServiceCollection AddApplicationHttpsRedirection(this IServiceCollection services)
        {
            services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = 443;
            });

            return services;
        }

        public static IServiceCollection AddApplicationHsts(this IServiceCollection services)
        {
            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
                options.Preload = true;
            });

            return services;
        }
    }
} 