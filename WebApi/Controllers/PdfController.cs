using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using UrbanLayoutGenerator.Configuration;
using UrbanLayoutGenerator.Core.Models;
using UrbanLayoutGenerator.Core.Services;
using UrbanLayoutGenerator.WebApi.Models;
using UrbanLayoutGenerator.WebApi.Services;

namespace UrbanLayoutGenerator.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfProcessor _processor;
        private readonly IProcessingStorage _storage;
        private readonly ILogger<PdfController> _logger;

        public PdfController(PdfProcessor processor, IProcessingStorage storage, ILogger<PdfController> logger)
        {
            _processor = processor;
            _storage = storage;
            _logger = logger;
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
                var processingId = Guid.NewGuid().ToString();
                var processingDir = Path.Combine(Path.GetTempPath(), "UrbanLayout", processingId);
                Directory.CreateDirectory(processingDir);

                var pdfPath = Path.Combine(processingDir, file.FileName);

                using (var stream = new FileStream(pdfPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var processingResult = new ProcessingResult
                {
                    Id = processingId,
                    OriginalFileName = file.FileName,
                    GeoJsonPath = Path.Combine(processingDir, "result.geojson"),
                    PassportPath = Path.Combine(processingDir, "passport.json"),
                    Status = "pending"
                };

                _storage.StoreResult(processingResult);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var processor = new PdfProcessor();
                        var result = await processor.ProcessPdfAsync(pdfPath);
                        await processor.ExportToGeoJsonAsync(result, processingResult.GeoJsonPath);
                        LibraryPassport.CreateDefault().SaveToFile(processingResult.PassportPath);

                        processingResult.TotalElements = result.TotalElements;
                        processingResult.ClassifiedCount = result.ClassifiedCount;
                        processingResult.Status = "completed";

                        _logger.LogInformation($"Фоновая обработка завершена. ID: {processingId}");
                    }
                    catch (Exception ex)
                    {
                        processingResult.Status = "error";
                        _logger.LogError(ex, $"Ошибка фоновой обработки ID: {processingId}");
                    }
                });

                return Ok(new UploadResponse { Id = processingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки PDF");
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [HttpGet("result/{id}/passport")]
        public IActionResult GetPassport(string id)
        {
            var result = _storage.GetResult(id);
            if (result == null)
                return NotFound(new { error = "Результат не найден" });

            if (!System.IO.File.Exists(result.PassportPath))
                return NotFound(new { error = "Файл паспорта не найден" });

            try
            {
                var jsonContent = System.IO.File.ReadAllText(result.PassportPath);
                var contentType = "application/json";
                var fileName = $"passport_{id}.json";

                return File(System.Text.Encoding.UTF8.GetBytes(jsonContent), contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Ошибка чтения файла: {ex.Message}" });
            }
        }

        [HttpGet("result/{id}/geojson")]
        public IActionResult GetGeoJson(string id)
        {
            var result = _storage.GetResult(id);
            if (result == null)
                return NotFound(new { error = "Результат не найден" });

            if (!System.IO.File.Exists(result.GeoJsonPath))
                return NotFound(new { error = "Файл GeoJSON не найден" });

            try
            {
                var jsonContent = System.IO.File.ReadAllText(result.GeoJsonPath);
                var contentType = "application/json";
                var fileName = $"result_{id}.geojson";

                return File(System.Text.Encoding.UTF8.GetBytes(jsonContent), contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Ошибка чтения файла: {ex.Message}" });
            }
        }

        [HttpGet("result/{id}/status")]
        public IActionResult GetStatus(string id)
        {
            var result = _storage.GetResult(id);
            if (result == null)
                return NotFound(new { error = "Результат не найден" });

            return Ok(new
            {
                id = result.Id,
                fileName = result.OriginalFileName,
                totalElements = result.TotalElements,
                classifiedElements = result.ClassifiedCount,
                status = result.Status,
                createdAt = result.CreatedAt,
                hasGeoJson = System.IO.File.Exists(result.GeoJsonPath),
                hasPassport = System.IO.File.Exists(result.PassportPath)
            });
        }
    }
}