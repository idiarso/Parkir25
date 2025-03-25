using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ParkIRC.Test.Models;
using ParkIRC.Test;

namespace ParkIRC.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Konfigurasi dependency injection
            var serviceProvider = ConfigureServices();

            // Mendapatkan logger
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("=== Aplikasi Test Printer Sederhana ===");

            // Mendapatkan printer service
            var printerService = serviceProvider.GetRequiredService<SimplifiedLinuxPrinterService>();

            try
            {
                // Inisialisasi printer
                logger.LogInformation("Menginisialisasi printer...");
                bool initialized = await printerService.InitializeAsync();
                
                if (!initialized)
                {
                    logger.LogError("Gagal menginisialisasi printer");
                    return;
                }

                // Test printer
                logger.LogInformation("Menjalankan test printer...");
                bool testResult = await printerService.TestPrinterAsync();
                
                if (!testResult)
                {
                    logger.LogError("Test printer gagal");
                    return;
                }

                // Mencetak contoh tiket
                logger.LogInformation("Mencetak contoh tiket...");
                var ticket = CreateSampleTicket();
                bool printResult = await printerService.PrintTicketAsync(ticket);
                
                if (!printResult)
                {
                    logger.LogError("Gagal mencetak tiket");
                    return;
                }

                // Mencetak contoh struk
                logger.LogInformation("Mencetak contoh struk...");
                bool receiptResult = await printerService.PrintReceiptAsync(
                    "TRX-12345",
                    "B1234CD",
                    DateTime.Now.AddHours(-2),
                    DateTime.Now,
                    "2 jam 0 menit",
                    15000);
                
                if (!receiptResult)
                {
                    logger.LogError("Gagal mencetak struk");
                    return;
                }

                logger.LogInformation("Semua test berhasil!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Terjadi kesalahan dalam aplikasi: {Message}", ex.Message);
            }

            Console.WriteLine("Tekan tombol apa saja untuk keluar...");
            Console.ReadKey();
        }

        private static Ticket CreateSampleTicket()
        {
            return new Ticket
            {
                TicketNumber = "TCK-" + DateTime.Now.ToString("yyyyMMddHHmm"),
                IssueTime = DateTime.Now,
                VehicleNumber = "B1234CD",
                VehicleType = "Mobil",
                ParkingSpace = "Lantai 1 - A5",
                EntryTime = DateTime.Now,
                Barcode = "978020137962"
            };
        }

        private static ServiceProvider ConfigureServices()
        {
            // Membuat konfigurasi dari appsettings.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Setup DI
            var services = new ServiceCollection()
                .AddLogging(config =>
                {
                    config.ClearProviders();
                    config.AddConsole();
                    config.SetMinimumLevel(LogLevel.Information);
                })
                .AddSingleton<IConfiguration>(configuration)
                .AddTransient<SimplifiedLinuxPrinterService>();

            // Menambahkan logger untuk Program
            services.AddTransient<ILogger<Program>>(provider =>
                provider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>());

            return services.BuildServiceProvider();
        }
    }
} 