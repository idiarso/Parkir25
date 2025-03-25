using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ParkIRC.Models;

namespace ParkIRC.Services
{
    public interface IOfflineDataService
    {
        Task SaveData(string data);
        Task SaveOfflineDataAsync(string data);
        Task<List<string>> GetPendingSyncDataAsync();
        Task MarkDataAsSyncedAsync(string id);
        Task<bool> HasPendingSyncDataAsync();
        Task ClearAllPendingDataAsync();
    }

    public class OfflineDataService : IOfflineDataService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<OfflineDataService> _logger;
        private readonly string _offlineDataPath;

        public OfflineDataService(IWebHostEnvironment webHostEnvironment, ILogger<OfflineDataService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _offlineDataPath = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data", "OfflineData");
            
            // Ensure directory exists
            if (!Directory.Exists(_offlineDataPath))
            {
                Directory.CreateDirectory(_offlineDataPath);
            }
        }

        public async Task SaveData(string data)
        {
            await SaveOfflineDataAsync(data);
        }

        public async Task SaveOfflineDataAsync(string data)
        {
            try
            {
                var fileName = $"offline_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}.json";
                var filePath = Path.Combine(_offlineDataPath, fileName);
                
                await File.WriteAllTextAsync(filePath, data);
                _logger.LogInformation($"Offline data saved to {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving offline data");
                throw;
            }
        }

        public async Task<List<string>> GetPendingSyncDataAsync()
        {
            try
            {
                var files = Directory.GetFiles(_offlineDataPath, "offline_*.json");
                var result = new List<string>();
                
                foreach (var file in files)
                {
                    var data = await File.ReadAllTextAsync(file);
                    result.Add(data);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending sync data");
                return new List<string>();
            }
        }

        public async Task MarkDataAsSyncedAsync(string id)
        {
            try
            {
                var files = Directory.GetFiles(_offlineDataPath, $"*{id}*.json");
                
                foreach (var file in files)
                {
                    var syncedFilePath = file.Replace("offline_", "synced_");
                    
                    // Move file to synced status
                    if (File.Exists(file))
                    {
                        File.Move(file, syncedFilePath, true);
                        _logger.LogInformation($"Marked data as synced: {file} -> {syncedFilePath}");
                    }
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking data as synced for ID {id}");
                throw;
            }
        }

        public async Task<bool> HasPendingSyncDataAsync()
        {
            try
            {
                var files = Directory.GetFiles(_offlineDataPath, "offline_*.json");
                return files.Length > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for pending sync data");
                return false;
            }
        }

        public async Task ClearAllPendingDataAsync()
        {
            try
            {
                var files = Directory.GetFiles(_offlineDataPath, "offline_*.json");
                
                foreach (var file in files)
                {
                    File.Delete(file);
                    _logger.LogInformation($"Deleted offline data file: {file}");
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing pending data");
                throw;
            }
        }
        
        public async Task ProcessOfflineTransaction(ParkingTransaction transaction)
        {
            try
            {
                var fileName = $"transaction_{transaction.TransactionNumber}_{DateTime.Now:yyyyMMddHHmmss}.json";
                var filePath = Path.Combine(_offlineDataPath, fileName);
                
                var json = JsonSerializer.Serialize(transaction, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                await File.WriteAllTextAsync(filePath, json);
                _logger.LogInformation($"Offline transaction saved to {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving offline transaction");
                throw;
            }
        }
        
        public async Task<List<ParkingTransaction>> GetPendingTransactionsAsync()
        {
            try
            {
                var files = Directory.GetFiles(_offlineDataPath, "transaction_*.json");
                var result = new List<ParkingTransaction>();
                
                foreach (var file in files)
                {
                    var json = await File.ReadAllTextAsync(file);
                    var transaction = JsonSerializer.Deserialize<ParkingTransaction>(json);
                    if (transaction != null)
                    {
                        result.Add(transaction);
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending transactions");
                return new List<ParkingTransaction>();
            }
        }
    }
} 