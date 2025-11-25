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
    public class GeoJsonExporter
    {
        public async Task<string> ExportAsync(List<RealPdfElement> elements, string outputPath)
        {
            var features = new List<Feature>();

            foreach (var element in elements)
            {
                if (element.Points.Count < 3) continue;

                var validatedPoints = ValidateAndClosePolygon(element.Points);
                if (validatedPoints.Count < 4) continue;

                var coordinates = new List<IPosition>();
                foreach (var point in validatedPoints)
                {
                    coordinates.Add(new Position(point.X, point.Y));
                }

                try
                {
                    var polygon = new Polygon(new List<LineString> { new LineString(coordinates) });

                    var properties = new Dictionary<string, object>
                    {
                        ["id"] = element.Id,
                        ["type"] = element.Type.ToString(),
                        ["fill_color"] = element.FillColor,
                        ["stroke_color"] = element.StrokeColor,
                        ["stroke_width"] = element.StrokeWidth,
                        ["area"] = CalculateArea(validatedPoints),
                        ["points_count"] = validatedPoints.Count
                    };
                    foreach (var prop in element.Properties)
                    {
                        if (prop.Value != null)
                            properties[prop.Key] = prop.Value;
                    }

                    var feature = new Feature(polygon, properties);
                    features.Add(feature);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Пропущен объект {element.Id}: {ex.Message}");
                    continue;
                }
            }

            if (features.Count == 0)
            {
                Console.WriteLine("Нет валидных объектов для экспорта в GeoJSON");
                features.Add(CreateFallbackFeature());
            }

            var featureCollection = new FeatureCollection(features);
            var geoJson = JsonConvert.SerializeObject(featureCollection, Formatting.Indented);
            await File.WriteAllTextAsync(outputPath, geoJson);

            Console.WriteLine($"GeoJSON экспортирован: {outputPath} ({features.Count} объектов)");
            return outputPath;
        }

        private List<PointD> ValidateAndClosePolygon(List<PointD> points)
        {
            if (points.Count < 3)
                return new List<PointD>();

            var validated = new List<PointD>(points);
            for (int i = validated.Count - 1; i > 0; i--)
            {
                if (Math.Abs(validated[i].X - validated[i - 1].X) < 0.001 &&
                    Math.Abs(validated[i].Y - validated[i - 1].Y) < 0.001)
                {
                    validated.RemoveAt(i);
                }
            }
            if (validated.Count >= 3)
            {
                var first = validated[0];
                var last = validated[validated.Count - 1];

                if (Math.Abs(first.X - last.X) > 0.001 || Math.Abs(first.Y - last.Y) > 0.001)
                {
                    validated.Add(new PointD(first.X, first.Y));
                }
            }

            return validated;
        }

        private Feature CreateFallbackFeature()
        {
            var coordinates = new List<IPosition>
            {
                new Position(0, 0),
                new Position(100, 0),
                new Position(100, 100),
                new Position(0, 100),
                new Position(0, 0)
            };

            var polygon = new Polygon(new List<LineString> { new LineString(coordinates) });

            var properties = new Dictionary<string, object>
            {
                ["id"] = "fallback",
                ["type"] = "Test",
                ["note"] = "Демо-объект, так как исходные данные невалидны"
            };

            return new Feature(polygon, properties);
        }

        private double CalculateArea(List<PointD> points)
        {
            if (points.Count < 3) return 0;

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