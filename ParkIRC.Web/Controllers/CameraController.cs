using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ParkIRC.Models;
using ParkIRC.Services;

namespace ParkIRC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CameraController : ControllerBase
    {
        private readonly ILogger<CameraController> _logger;
        private readonly IIPCameraService _ipCameraService;
        private readonly ICameraService _cameraService;

        public CameraController(
            ILogger<CameraController> logger,
            IIPCameraService ipCameraService,
            ICameraService cameraService)
        {
            _logger = logger;
            _ipCameraService = ipCameraService;
            _cameraService = cameraService;
        }

        /// <summary>
        /// Get a snapshot from the camera
        /// </summary>
        [HttpGet("snapshot")]
        [AllowAnonymous] // Allowing anonymous access for testing purposes
        public async Task<IActionResult> GetSnapshot()
        {
            try
            {
                _logger.LogInformation("Getting snapshot from camera");
                
                // Initialize if not already done
                if (!await _ipCameraService.InitializeAsync())
                {
                    _logger.LogWarning("Failed to initialize IP camera service");
                    return StatusCode(500, "Failed to initialize camera. Please check configuration.");
                }
                
                var imageBytes = await _ipCameraService.GetSnapshotAsync();
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    _logger.LogWarning("No image data received from camera");
                    return NotFound("No image data received from camera");
                }
                
                return File(imageBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting snapshot from camera");
                return StatusCode(500, "Error getting snapshot from camera: " + ex.Message);
            }
        }

        /// <summary>
        /// Capture and save an image from the camera
        /// </summary>
        [HttpPost("capture")]
        [Authorize]
        public async Task<IActionResult> CaptureImage()
        {
            try
            {
                _logger.LogInformation("Capturing image from camera");
                
                // Initialize if not already done
                if (!await _ipCameraService.InitializeAsync())
                {
                    _logger.LogWarning("Failed to initialize IP camera service");
                    return StatusCode(500, "Failed to initialize camera. Please check configuration.");
                }
                
                var imagePath = await _ipCameraService.CaptureImageAsync();
                if (string.IsNullOrEmpty(imagePath))
                {
                    _logger.LogWarning("Failed to capture and save image");
                    return StatusCode(500, "Failed to capture and save image");
                }
                
                return Ok(new { success = true, imagePath = imagePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing image from camera");
                return StatusCode(500, "Error capturing image from camera: " + ex.Message);
            }
        }

        /// <summary>
        /// Get the URL for the camera stream
        /// </summary>
        [HttpGet("stream")]
        [AllowAnonymous] // Allowing anonymous access for testing purposes
        public async Task<IActionResult> GetStreamUrl()
        {
            try
            {
                _logger.LogInformation("Getting camera stream URL");
                
                // Initialize if not already done
                if (!await _ipCameraService.InitializeAsync())
                {
                    _logger.LogWarning("Failed to initialize IP camera service");
                    return StatusCode(500, "Failed to initialize camera. Please check configuration.");
                }
                
                var streamUrl = await _ipCameraService.GetStreamUrlAsync();
                return Ok(new { streamUrl = streamUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting camera stream URL");
                return StatusCode(500, "Error getting camera stream URL: " + ex.Message);
            }
        }

        /// <summary>
        /// Test the camera connection
        /// </summary>
        [HttpGet("test")]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                _logger.LogInformation("Testing camera connection");
                
                var result = await _ipCameraService.TestConnectionAsync();
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing camera connection");
                return StatusCode(500, "Error testing camera connection: " + ex.Message);
            }
        }

        /// <summary>
        /// Get current camera settings from the database
        /// </summary>
        [HttpGet("settings")]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> GetCameraSettings()
        {
            try
            {
                var settings = await _cameraService.GetSettingsAsync();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting camera settings");
                return StatusCode(500, "Error getting camera settings: " + ex.Message);
            }
        }

        /// <summary>
        /// Update camera settings in the database
        /// </summary>
        [HttpPost("settings")]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> UpdateCameraSettings(CameraSettings settings)
        {
            try
            {
                var result = await _cameraService.UpdateSettingsAsync(settings);
                if (result)
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return StatusCode(500, "Failed to update camera settings");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating camera settings");
                return StatusCode(500, "Error updating camera settings: " + ex.Message);
            }
        }
    }
} 