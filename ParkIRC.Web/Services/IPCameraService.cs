using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkIRC.Models;

namespace ParkIRC.Services
{
    public interface IIPCameraService
    {
        Task<bool> InitializeAsync();
        Task<string> CaptureImageAsync(string? savePath = null);
        Task<byte[]> GetSnapshotAsync();
        Task<string> GetStreamUrlAsync();
        Task<bool> TestConnectionAsync();
    }

    public class IPCameraService : IIPCameraService
    {
        private readonly ILogger<IPCameraService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private string _snapshotUrl;
        private string _streamUrl;
        private string _savePath;
        private bool _isInitialized;

        public IPCameraService(ILogger<IPCameraService> logger, IConfiguration configuration, HttpClient? httpClient = null)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient ?? new HttpClient();
            _baseUrl = _configuration["IPCamera:BaseUrl"] ?? throw new ArgumentNullException(nameof(configuration), "IPCamera:BaseUrl is not configured");
            _username = _configuration["IPCamera:Username"] ?? throw new ArgumentNullException(nameof(configuration), "IPCamera:Username is not configured");
            _password = _configuration["IPCamera:Password"] ?? throw new ArgumentNullException(nameof(configuration), "IPCamera:Password is not configured");
            _isInitialized = false;
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing IP camera service...");
                
                // Get camera settings from configuration
                var cameraSettings = _configuration.GetSection("CameraSettings:IpCamera");
                _snapshotUrl = cameraSettings["SnapshotUrl"] ?? "/snapshot.jpg";
                _streamUrl = cameraSettings["StreamUrl"] ?? "/stream";
                
                // Get save path for captured images
                _savePath = _configuration.GetSection("CameraSettings:SavePath").Value ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "captures");
                
                // Ensure directory exists
                if (!Directory.Exists(_savePath))
                {
                    Directory.CreateDirectory(_savePath);
                }
                
                // Set basic authentication for the HTTP client
                var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                
                // Test the connection
                var connectionResult = await TestConnectionAsync();
                _isInitialized = connectionResult;
                
                if (_isInitialized)
                {
                    _logger.LogInformation("IP camera service initialized successfully");
                }
                else
                {
                    _logger.LogWarning("IP camera initialization failed - could not connect to camera");
                }
                
                return _isInitialized;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize IP camera service");
                return false;
            }
        }

        public async Task<string> CaptureImageAsync(string? savePath = null)
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("IP camera service is not initialized");
                }
            }

            try
            {
                _logger.LogInformation("Capturing image from IP camera...");
                
                var imageBytes = await GetSnapshotAsync();
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    _logger.LogWarning("Failed to capture image - no data received from camera");
                    return null;
                }
                
                // Create filename with timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string filename = $"capture_{timestamp}.jpg";
                string directoryPath = savePath ?? _savePath;
                string filePath = Path.Combine(directoryPath, filename);
                
                // Save the image
                await File.WriteAllBytesAsync(filePath, imageBytes);
                
                _logger.LogInformation($"Image captured and saved to {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture image from IP camera");
                return null;
            }
        }

        public async Task<byte[]> GetSnapshotAsync()
        {
            try
            {
                string url = _baseUrl.TrimEnd('/') + _snapshotUrl;
                _logger.LogDebug($"Getting snapshot from {url}");
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting snapshot from IP camera");
                return null;
            }
        }

        public async Task<string> GetStreamUrlAsync()
        {
            return _baseUrl.TrimEnd('/') + _streamUrl;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing IP camera connection...");
                
                string url = _baseUrl.TrimEnd('/') + _snapshotUrl;
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("IP camera connection test successful");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"IP camera connection test failed with status: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test IP camera connection");
                return false;
            }
        }
    }
} 