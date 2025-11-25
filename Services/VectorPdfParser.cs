using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UrbanLayoutGenerator.Models;
using UrbanLayoutGenerator.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Drawing;

namespace UrbanLayoutGenerator.Services
{
    public class VectorPdfParser : IPdfParser
    {
        public async Task<List<SiteElement>> ParseAsync(string pdfPath)
        {
            return await Task.Run(() =>
            {
                var elements = new List<SiteElement>();

                using var document = PdfDocument.Open(pdfPath);

                foreach (var page in document.GetPages())
                {
                    Console.WriteLine($"Обработка страницы {page.Number}");

                    // Обрабатываем пути (paths)
                    ProcessPaths(page, elements);

                    // Обрабатываем текст (для будущего расширения)
                    ProcessText(page, elements);
                }

                return elements;
            });
        }

        private void ProcessPaths(Page page, List<SiteElement> elements)
        {
            // PdfPig не предоставляет прямой доступ к цветам путей в бесплатной версии
            // В реальном проекте нужно использовать библиотеку с поддержкой цветов
            // Например, iTextSharp (платная) или PdfiumViewer

            // Эмуляция извлечения геометрии
            ExtractGeometryFromPage(page, elements);
        }

        private void ExtractGeometryFromPage(Page page, List<SiteElement> elements)
        {
            // В этом MVP эмулируем извлечение геометрии
            // В реальном проекте здесь будет сложная логика анализа PDF-примитивов

            var random = new Random();

            // Эмуляция обнаружения зданий
            for (int i = 0; i < 5; i++)
            {
                var building = new SiteElement
                {
                    Type = ElementType.Building,
                    OriginalColor = "#0000FF",
                    Points = GenerateRandomPolygon(100 + i * 200, 100 + i * 150, 80, 60)
                };
                elements.Add(building);
            }

            // Эмуляция границы участка
            var boundary = new SiteElement
            {
                Type = ElementType.SiteBoundary,
                OriginalColor = "#000000",
                Points = new List<GeometryPoint>
                {
                    new(50, 50),
                    new(800, 50),
                    new(800, 600),
                    new(50, 600),
                    new(50, 50)
                }
            };
            elements.Add(boundary);

            // Эмуляция красных линий
            var redLine = new SiteElement
            {
                Type = ElementType.RedLine,
                OriginalColor = "#FF0000",
                Points = new List<GeometryPoint>
                {
                    new(30, 30),
                    new(820, 30),
                    new(820, 620),
                    new(30, 620),
                    new(30, 30)
                }
            };
            elements.Add(redLine);

            // Эмуляция детской площадки
            var playground = new SiteElement
            {
                Type = ElementType.PlaygroundChildren,
                OriginalColor = "#FF00FF",
                Points = GenerateRandomPolygon(300, 400, 40, 40)
            };
            elements.Add(playground);
        }

        private List<GeometryPoint> GenerateRandomPolygon(double centerX, double centerY, double width, double height)
        {
            var points = new List<GeometryPoint>();
            var random = new Random();

            points.Add(new GeometryPoint(centerX - width / 2 + random.Next(-10, 10), centerY - height / 2 + random.Next(-10, 10)));
            points.Add(new GeometryPoint(centerX + width / 2 + random.Next(-10, 10), centerY - height / 2 + random.Next(-10, 10)));
            points.Add(new GeometryPoint(centerX + width / 2 + random.Next(-10, 10), centerY + height / 2 + random.Next(-10, 10)));
            points.Add(new GeometryPoint(centerX - width / 2 + random.Next(-10, 10), centerY + height / 2 + random.Next(-10, 10)));
            points.Add(points[0]); // Замыкаем полигон

            return points;
        }

        private void ProcessText(Page page, List<SiteElement> elements)
        {
            // Для будущего расширения: извлечение подписей, этажности и т.д.
            foreach (var word in page.GetWords())
            {
                // Анализ текста для определения характеристик объектов
                Console.WriteLine($"Найден текст: {word.Text} на позиции {word.BoundingBox}");
            }
        }
    }
}