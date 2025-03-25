using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ParkIRC.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;

        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }

        [HttpPost("upload/image")]
        public async Task<IActionResult> UploadImage([FromBody] ImageUploadRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Data))
                {
                    return BadRequest(new { success = false, message = "No image data provided" });
                }

                // Extract base64 data
                var base64Data = request.Data.Substring(request.Data.IndexOf(",") + 1);
                var imageBytes = Convert.FromBase64String(base64Data);

                // Create directory if it doesn't exist
                string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", request.Type);
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                // Generate unique filename
                string fileName = $"{request.Type}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}.jpg";
                string filePath = Path.Combine(uploadDir, fileName);
                
                // Save the file
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                
                // Return the relative path for storing in database
                string relativePath = $"/uploads/{request.Type}/{fileName}";
                
                _logger.LogInformation($"Image uploaded successfully: {relativePath}");
                return Ok(new { success = true, filePath = relativePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new { success = false, message = "Error uploading image" });
            }
        }
    }

    public class ImageUploadRequest
    {
        public string Data { get; set; } = string.Empty;
        public string Type { get; set; } = "entry"; // entry or exit
    }
} 