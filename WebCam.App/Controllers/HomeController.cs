using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebCam.App.Models;

namespace WebCam.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route("Home")]
        [Route("Home/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/UploadImage")]
        public IActionResult UploadImage([FromBody] ImageDataModel imageData)
        {
            try
            {
                // Validate incoming data
                if (imageData == null || string.IsNullOrEmpty(imageData.Base64Data))
                {
                    return BadRequest(new { Message = "Invalid image data" });
                }

                // Remove data URI prefix if present
                string base64Data = imageData.Base64Data.Replace("data:image/jpeg;base64,", "");

                // Remove whitespace characters
                base64Data = base64Data.Replace(" ", "");

                // Replace URL-safe characters
                base64Data = base64Data.Replace('-', '+').Replace('_', '/');

                // Add padding if needed
                int padding = base64Data.Length % 4;
                if (padding > 0)
                {
                    base64Data += new string('=', 4 - padding);
                }

                // Decode the base64 data into a byte array
                byte[] imageBytes = Convert.FromBase64String(base64Data);

                // Generate a unique file name using ticks
                string fileName = $"{DateTime.Now.Ticks}.jpg";

                // Combine the file path with the wwwroot/images directory
                string filePath = Path.Combine("wwwroot", "images", fileName);

                // Save the image bytes to a file
                System.IO.File.WriteAllBytes(filePath, imageBytes);

                // Return a success response
                return Ok(new { Message = "Image uploaded successfully", FileName = fileName });
            }
            catch (Exception ex)
            {
                // Return a detailed error response
                return BadRequest(new { Message = $"Failed to upload image. Error: {ex.Message}" });
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    public class ImageDataModel
    {
        public string Base64Data { get; set; }
    }
}

