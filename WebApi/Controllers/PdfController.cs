using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using UrbanLayoutGenerator.Core.Services;

namespace UrbanLayoutGenerator.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfProcessor _processor;

        public PdfController()
        {
            _processor = new PdfProcessor();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен");

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Только PDF файлы");

            try
            {
                var tempPath = Path.GetTempFileName() + ".pdf";
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var result = await _processor.ProcessPdfAsync(tempPath);
                var geoJsonPath = Path.ChangeExtension(tempPath, ".geojson");
                await _processor.ExportToGeoJsonAsync(result, geoJsonPath);

                var geoJsonContent = await System.IO.File.ReadAllTextAsync(geoJsonPath);

                System.IO.File.Delete(tempPath);
                System.IO.File.Delete(geoJsonPath);

                return Ok(new
                {
                    success = true,
                    message = $"Обработано объектов: {result.TotalElements}",
                    data = geoJsonContent,
                    statistics = new
                    {
                        total = result.TotalElements,
                        classified = result.ClassifiedCount
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка обработки: {ex.Message}");
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "API работает", timestamp = DateTime.UtcNow });
        }
    }
}