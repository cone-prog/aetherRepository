// Program.cs
using System;
using System.IO;
using System.Threading.Tasks;
using UrbanLayoutGenerator.Services;

namespace UrbanLayoutGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== PDF Parser MVP для генерации застройки ===\n");

            try
            {
                // Конфигурация зависимостей
                var pdfParser = new VectorPdfParser();
                var geoJsonExporter = new GeoJsonExporter();
                var processor = new PlotPlanProcessor(pdfParser, geoJsonExporter);

                // Обработка PDF
                string pdfPath = GetPdfPath(args);
                string resultPath = await processor.ProcessAsync(pdfPath);

                Console.WriteLine($"\nОбработка завершена успешно!");
                Console.WriteLine($"GeoJSON файл: {resultPath}");
                Console.WriteLine($"Откройте файл в QGIS для визуальной проверки.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine($"Детали: {ex}");
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        private static string GetPdfPath(string[] args)
        {
            if (args.Length > 0 && File.Exists(args[0]))
                return args[0];

            Console.Write("Введите путь к PDF файлу: ");
            var path = Console.ReadLine();

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                // Демо-режим с эмуляцией данных
                Console.WriteLine("Файл не найден. Запуск в демо-режиме...");
                return "demo";
            }

            return path;
        }
    }
}