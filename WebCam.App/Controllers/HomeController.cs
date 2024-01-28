using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebCam.App.Models;

namespace WebCam.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("Home/Privacy")]
        public IActionResult Privacy()
        {
            string wwwRootPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");

            var docs = GetAllImages(wwwRootPath);
            return View(docs);
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


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public List<ImagesData> GetAllImages(string directoryPath)
        {
            List<string> imagePaths = new List<string>();

            try
            {
                // Check if the directory exists
                if (Directory.Exists(directoryPath))
                {
                    // Get all image files in the directory
                    string[] files = Directory.GetFiles(directoryPath, "*.png");
                    imagePaths.AddRange(files);

                    files = Directory.GetFiles(directoryPath, "*.jpg");
                    imagePaths.AddRange(files);

                    files = Directory.GetFiles(directoryPath, "*.jpeg");
                    imagePaths.AddRange(files);

                    files = Directory.GetFiles(directoryPath, "*.gif");
                    imagePaths.AddRange(files);

                    files = Directory.GetFiles(directoryPath, "*.bmp");
                    imagePaths.AddRange(files);

                    files = Directory.GetFiles(directoryPath, "*.ico");
                    imagePaths.AddRange(files);

                    files = Directory.GetFiles(directoryPath, "*.svg");
                    imagePaths.AddRange(files);

                    // You can add more file extensions as needed

                    //// Recursively get images from subdirectories
                    //string[] directories = Directory.GetDirectories(directoryPath);
                    //foreach (string directory in directories)
                    //{
                    //    imagePaths.AddRange(GetAllImages(directory));
                    //}
                }
                else
                {
                    Console.WriteLine("Directory does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            var images = ConvertToVirtualPaths(imagePaths);
            return images;
        }
        private List<ImagesData> ConvertToVirtualPaths(List<string> physicalPaths)
        {
            List<ImagesData> virtualPaths = new List<ImagesData>();
            var url = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host.ToString();
            string wwwRootPath = Path.Combine(_hostingEnvironment.WebRootPath);

            foreach (string physicalPath in physicalPaths)
            {
                ImagesData images = new ImagesData();
                string virtualPath = physicalPath.Replace(wwwRootPath, url).Replace("\\", "/");
                string filename = Path.GetFileName(physicalPath);
                string virtualPathWithFilename = $"{virtualPath}";
                images.Path = $"{virtualPathWithFilename}";
                images.Name = filename;

                virtualPaths.Add(images);
            };

            return virtualPaths;
        }
    }

    public class ImageDataModel
    {
        public string Base64Data { get; set; }
    }
    public class ImagesData
    {
        public string Path { get; set; }
        public string Name { get; set; }
    }
}

