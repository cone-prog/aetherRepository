using System;
using System.IO;
using System.Threading.Tasks;
using UrbanLayoutGenerator.Configuration;
using UrbanLayoutGenerator.Core.Services;

namespace UrbanLayoutGenerator.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                string pdfPath = GetPdfPathFromUser(args);

                if (!File.Exists(pdfPath))
                {
                    System.Console.WriteLine($"Файл не найден: {pdfPath}");
                    return;
                }

                System.Console.WriteLine($"Обработка: {Path.GetFileName(pdfPath)}");

                var processor = new PdfProcessor();
                var result = await processor.ProcessPdfAsync(pdfPath);

                var outputPath = Path.ChangeExtension(pdfPath, ".geojson");
                var geoJsonPath = await processor.ExportToGeoJsonAsync(result, outputPath);

                var passportPath = Path.ChangeExtension(pdfPath, ".passport.json");
                LibraryPassport.CreateDefault().SaveToFile(passportPath);

                PrintResults(result, passportPath, geoJsonPath);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"ОШИБКА: {ex.Message}");
            }
        }

        static void PrintResults(Core.Models.ParsingResult result, string passportPath, string geoJsonPath)
        {
            System.Console.WriteLine("\n=== РЕЗУЛЬТАТЫ ===");
            System.Console.WriteLine($"Всего объектов: {result.TotalElements}");
            System.Console.WriteLine($"Классифицировано: {result.ClassifiedCount}");

            System.Console.WriteLine($"\nФайлы созданы:");
            System.Console.WriteLine($"• GeoJSON: {geoJsonPath}");
            System.Console.WriteLine($"• Паспорт: {passportPath}");
            System.Console.WriteLine($"\nОткройте GeoJSON в QGIS для просмотра");
        }

        static string GetPdfPathFromUser(string[] args)
        {
            if (args.Length > 0 && File.Exists(args[0]))
                return args[0];

            System.Console.Write("Введите путь к PDF: ");
            return System.Console.ReadLine()?.Trim('"') ?? "";
        }
    }
}