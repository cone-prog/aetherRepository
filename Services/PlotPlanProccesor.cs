// Services/PlotPlanProcessor.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UrbanLayoutGenerator.Models;
using UrbanLayoutGenerator.Services;


namespace UrbanLayoutGenerator.Services
{
    public class PlotPlanProcessor
    {
        private readonly IPdfParser _pdfParser;
        private readonly IGeoJsonExporter _geoJsonExporter;

        public PlotPlanProcessor(IPdfParser pdfParser, IGeoJsonExporter geoJsonExporter)
        {
            _pdfParser = pdfParser;
            _geoJsonExporter = geoJsonExporter;
        }

        public async Task<string> ProcessAsync(string pdfPath, string outputPath = null)
        {
            if (!File.Exists(pdfPath))
                throw new FileNotFoundException($"PDF файл не найден: {pdfPath}");

            outputPath ??= Path.ChangeExtension(pdfPath, ".geojson");

            Console.WriteLine($"Начало обработки: {pdfPath}");

            // 1. Парсинг PDF
            var elements = await _pdfParser.ParseAsync(pdfPath);
            Console.WriteLine($"Извлечено объектов: {elements.Count}");

            // 2. Экспорт в GeoJSON
            var resultPath = await _geoJsonExporter.ExportAsync(elements, outputPath);
            Console.WriteLine($"Результат сохранен: {resultPath}");

            // 3. Статистика
            PrintStatistics(elements);

            return resultPath;
        }

        private void PrintStatistics(List<SiteElement> elements)
        {
            var stats = elements.GroupBy(e => e.Type)
                               .Select(g => new { Type = g.Key, Count = g.Count() });

            Console.WriteLine("\n=== СТАТИСТИКА ===");
            foreach (var stat in stats)
            {
                Console.WriteLine($"{stat.Type}: {stat.Count} объектов");
            }
        }
    }
}