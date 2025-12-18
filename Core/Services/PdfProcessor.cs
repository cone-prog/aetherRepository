using System;
using System.IO;
using System.Threading.Tasks;
using UrbanLayoutGenerator.Configuration;
using UrbanLayoutGenerator.Core.Models;
using UrbanLayoutGenerator.Services;

namespace UrbanLayoutGenerator.Core.Services
{
    public class PdfProcessor : IPdfProcessor
    {
        private readonly PdfToSvgConverter _pdfConverter;
        private readonly AdvancedSvgParser _svgParser;
        private readonly GeoJsonExporter _geoExporter;
        private readonly LibraryPassport _passport;

        public PdfProcessor(LibraryPassport passport = null)
        {
            _pdfConverter = new PdfToSvgConverter();
            _svgParser = new AdvancedSvgParser(passport ?? LibraryPassport.CreateDefault());
            _geoExporter = new GeoJsonExporter();
            _passport = passport ?? LibraryPassport.CreateDefault();
        }

        public async Task<ParsingResult> ProcessPdfAsync(string pdfPath)
        {
            if (!File.Exists(pdfPath))
                throw new FileNotFoundException($"PDF файл не найден: {pdfPath}");

            try
            {
                Console.WriteLine($"Начало обработки: {Path.GetFileName(pdfPath)}");

                var svgPath = await ConvertToSvgAsync(pdfPath);

                var result = _svgParser.ParseSvg(svgPath);

                Console.WriteLine($"Обработка завершена. Объектов: {result.TotalElements}");
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка обработки PDF: {ex.Message}", ex);
            }
        }

        public async Task<string> ConvertToSvgAsync(string pdfPath)
        {
            if (!File.Exists(pdfPath))
                throw new FileNotFoundException($"PDF файл не найден: {pdfPath}");

            var outputDir = Path.GetDirectoryName(pdfPath) ?? ".";
            return await _pdfConverter.ConvertToSvgAsync(pdfPath, outputDir);
        }

        public async Task<string> ExportToGeoJsonAsync(ParsingResult result, string outputPath = null)
        {
            if (result == null || result.Elements.Count == 0)
                throw new ArgumentException("Нет данных для экспорта");

            if (string.IsNullOrEmpty(outputPath))
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                outputPath = $"output_{timestamp}.geojson";
            }

            return await _geoExporter.ExportAsync(result.Elements, outputPath);
        }

        public async Task<string> ProcessAndExportAsync(string pdfPath, string outputPath = null)
        {
            var result = await ProcessPdfAsync(pdfPath);
            return await ExportToGeoJsonAsync(result, outputPath);
        }
    }
}