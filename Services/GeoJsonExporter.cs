using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UrbanLayoutGenerator.Models;

namespace UrbanLayoutGenerator.Services
{
    public class GeoJsonExporter : IGeoJsonExporter
    {
        public async Task<string> ExportAsync(List<SiteElement> elements, string outputPath)
        {
            var features = new List<Feature>();

            foreach (var element in elements)
            {
                if (element.Points.Count < 3) continue;

                // Конвертируем точки в координаты GeoJSON
                var coordinates = new List<IPosition>();
                foreach (var point in element.Points)
                {
                    coordinates.Add(new Position(point.X, point.Y));
                }

                // Создаем полигон (упрощенно - без отверстий)
                var polygon = new Polygon(new List<LineString>
                {
                    new LineString(coordinates)
                });

                var properties = new Dictionary<string, object>
                {
                    ["id"] = element.Id,
                    ["type"] = element.Type.ToString(),
                    ["originalColor"] = element.OriginalColor,
                    ["area"] = CalculateArea(element.Points)
                };

                // Добавляем дополнительные свойства
                foreach (var prop in element.Properties)
                {
                    properties[prop.Key] = prop.Value;
                }

                var feature = new Feature(polygon, properties);
                features.Add(feature);
            }

            var featureCollection = new FeatureCollection(features);
            var geoJson = JsonConvert.SerializeObject(featureCollection, Formatting.Indented);

            await File.WriteAllTextAsync(outputPath, geoJson);
            return outputPath;
        }

        private double CalculateArea(List<GeometryPoint> points)
        {
            // Упрощенный расчет площади (метод трапеций)
            double area = 0;
            int n = points.Count;

            for (int i = 0; i < n - 1; i++)
            {
                area += (points[i].X * points[i + 1].Y - points[i + 1].X * points[i].Y);
            }

            return Math.Abs(area / 2.0);
        }
    }
}