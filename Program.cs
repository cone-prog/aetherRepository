
using System;
using System.IO;
using System.Linq;
using UrbanLayoutGenerator.Configuration;
using UrbanLayoutGenerator.Services;
using UrbanLayoutGenerator.Models;

try
{
    string pdfPath = GetPdfPathFromUser(args);

    if (!File.Exists(pdfPath))
    {
        Console.WriteLine($"Файл не найден: {pdfPath}");
        Console.WriteLine("Убедитесь, что файл существует и путь указан правильно.");
        return;
    }

    Console.WriteLine($"Обработка файла: {Path.GetFileName(pdfPath)}");
    Console.WriteLine("Конвертация PDF в SVG...");
    var pdfConverter = new PdfToSvgConverter();
    var svgPath = await pdfConverter.ConvertToSvgAsync(pdfPath);
    Console.WriteLine("Парсинг SVG и классификация объектов...");
    var passport = LibraryPassport.CreateDefault();
    var svgParser = new AdvancedSvgParser(passport);
    var result = svgParser.ParseSvg(svgPath);
    Console.WriteLine("Экспорт в GeoJSON...");
    var geoExporter = new GeoJsonExporter();
    var outputPath = Path.ChangeExtension(pdfPath, ".geojson");
    var geoJsonPath = await geoExporter.ExportAsync(result.Elements, outputPath);
    var passportPath = Path.ChangeExtension(pdfPath, ".passport.json");
    passport.SaveToFile(passportPath);
    Console.WriteLine("РЕЗУЛЬТАТЫ ОБРАБОТКИ");

    Console.WriteLine($"Всего объектов: {result.TotalElements}");
    Console.WriteLine($"Классифицировано: {result.ClassifiedCount}");
    Console.WriteLine($"Не классифицировано: {result.UnclassifiedCount}");

    var stats = result.Elements
        .Where(e => e.Type != ElementType.Unknown)
        .GroupBy(e => e.Type)
        .Select(g => new { Type = g.Key, Count = g.Count() })
        .OrderByDescending(s => s.Count);

    Console.WriteLine("\nСтатистика по типам объектов:");
    foreach (var stat in stats)
    {
        var symbol = passport.Symbols.Values.FirstOrDefault(s => s.Type == stat.Type);
        var name = symbol?.Name ?? "Неизвестно";
        Console.WriteLine($"    {name}: {stat.Count} объектов");
    }

    Console.WriteLine($"\nСозданные файлы:");
    Console.WriteLine($"GeoJSON для QGIS: {geoJsonPath}");
    Console.WriteLine($"Паспорт библиотеки: {passportPath}");
    Console.WriteLine($"Промежуточный SVG: {svgPath}");

    Console.WriteLine($"Обработка завершена успешно!");
    Console.WriteLine($"Откройте файл {geoJsonPath} в QGIS для просмотра результатов.");
}
catch (Exception ex)
{
    Console.WriteLine($"ОШИБКА: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Детали: {ex.InnerException.Message}");
    }
}

Console.WriteLine("\nНажмите любую клавишу для выхода...");
Console.ReadKey();

static string GetPdfPathFromUser(string[] args)
{
    if (args.Length > 0 && File.Exists(args[0]))
    {
        return args[0];
    }

    Console.Write("Введите путь к PDF файлу: ");
    string inputPath = Console.ReadLine()?.Trim('"').Trim() ?? "";


    inputPath = inputPath.Trim('"');
    
    if (!inputPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine(" Файл должен иметь расширение .pdf");
    }

    return inputPath;
}