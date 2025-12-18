using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UrbanLayoutGenerator.Configuration;
using UrbanLayoutGenerator.Core.Models;

namespace UrbanLayoutGenerator.Services
{
    public class AdvancedSvgParser
    {
        private readonly LibraryPassport _passport;
        private readonly List<TextElement> _textElements = new();

        public AdvancedSvgParser(LibraryPassport passport)
        {
            _passport = passport;
        }

        public ParsingResult ParseSvg(string svgPath)
        {
            var elements = new List<RealPdfElement>();
            _textElements.Clear();

            if (!File.Exists(svgPath))
                throw new FileNotFoundException($"SVG файл не найден: {svgPath}");

            var svgDoc = XDocument.Load(svgPath);
            var ns = svgDoc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            Console.WriteLine("парсинг svg");

            ExtractAllText(svgDoc, ns);
            ParseGraphicElements(svgDoc, ns, elements);
            AssociateTextWithElements(elements);
            RefineClassification(elements);

            return new ParsingResult
            {
                Elements = elements,
                TextElements = _textElements,
                UnclassifiedCount = elements.Count(e => e.Type == ElementType.Unknown)
            };
        }

        private void ParseGraphicElements(XDocument doc, XNamespace ns, List<RealPdfElement> elements)
        {
            var paths = doc.Descendants(ns + "path").ToList();
            Console.WriteLine($"Найдено paths: {paths.Count}");
            foreach (var path in paths)
            {
                var element = ParsePathElement(path);
                if (element != null) elements.Add(element);
            }
            var polygons = doc.Descendants(ns + "polygon").Concat(doc.Descendants(ns + "polyline"));
            foreach (var poly in polygons)
            {
                var element = ParsePolygonElement(poly);
                if (element != null) elements.Add(element);
            }
            var rects = doc.Descendants(ns + "rect");
            foreach (var rect in rects)
            {
                var element = ParseRectElement(rect);
                if (element != null) elements.Add(element);
            }
            var circles = doc.Descendants(ns + "circle").Concat(doc.Descendants(ns + "ellipse"));
            foreach (var circle in circles)
            {
                var element = ParseCircleElement(circle);
                if (element != null) elements.Add(element);
            }
        }

        private RealPdfElement ParsePathElement(XElement path)
        {
            var d = path.Attribute("d")?.Value;
            if (string.IsNullOrEmpty(d)) return null;

            var element = new RealPdfElement
            {
                FillColor = NormalizeColor(path.Attribute("fill")?.Value),
                StrokeColor = NormalizeColor(path.Attribute("stroke")?.Value),
                StrokeWidth = ParseDouble(path.Attribute("stroke-width")?.Value) ?? 1.0,
                StrokeDashArray = path.Attribute("stroke-dasharray")?.Value,
                Points = ParsePathData(d)
            };

            element.Type = ClassifyElement(element);
            element.Properties["svg_type"] = "path";
            element.Properties["path_length"] = CalculatePathLength(element.Points);

            return element.Points.Count >= 3 ? element : null;
        }

        private RealPdfElement ParsePolygonElement(XElement poly)
        {
            var pointsAttr = poly.Attribute("points")?.Value;
            if (string.IsNullOrEmpty(pointsAttr)) return null;

            var element = new RealPdfElement
            {
                FillColor = NormalizeColor(poly.Attribute("fill")?.Value),
                StrokeColor = NormalizeColor(poly.Attribute("stroke")?.Value),
                StrokeWidth = ParseDouble(poly.Attribute("stroke-width")?.Value) ?? 1.0,
                StrokeDashArray = poly.Attribute("stroke-dasharray")?.Value,
                Points = ParsePointsAttribute(pointsAttr)
            };

            element.Type = ClassifyElement(element);
            element.Properties["svg_type"] = poly.Name.LocalName;
            element.Properties["area"] = CalculateArea(element.Points);

            return element.Points.Count >= 3 ? element : null;
        }

        private RealPdfElement ParseRectElement(XElement rect)
        {
            var x = ParseDouble(rect.Attribute("x")?.Value) ?? 0;
            var y = ParseDouble(rect.Attribute("y")?.Value) ?? 0;
            var width = ParseDouble(rect.Attribute("width")?.Value) ?? 0;
            var height = ParseDouble(rect.Attribute("height")?.Value) ?? 0;

            if (width == 0 || height == 0) return null;

            var points = new List<PointD>
            {
                new(x, y),
                new(x + width, y),
                new(x + width, y + height),
                new(x, y + height),
                new(x, y)
            };

            var element = new RealPdfElement
            {
                FillColor = NormalizeColor(rect.Attribute("fill")?.Value),
                StrokeColor = NormalizeColor(rect.Attribute("stroke")?.Value),
                StrokeWidth = ParseDouble(rect.Attribute("stroke-width")?.Value) ?? 1.0,
                StrokeDashArray = rect.Attribute("stroke-dasharray")?.Value,
                Points = points
            };

            element.Type = ClassifyElement(element);
            element.Properties["svg_type"] = "rect";
            element.Properties["area"] = width * height;
            element.Properties["aspect_ratio"] = width / height;

            return element;
        }

        private RealPdfElement ParseCircleElement(XElement circle)
        {
            var cx = ParseDouble(circle.Attribute("cx")?.Value) ?? 0;
            var cy = ParseDouble(circle.Attribute("cy")?.Value) ?? 0;
            var r = ParseDouble(circle.Attribute("r")?.Value) ?? 0;

            if (r == 0) return null;

            var points = new List<PointD>();
            for (int i = 0; i <= 16; i++)
            {
                var angle = 2 * Math.PI * i / 16;
                points.Add(new PointD(cx + r * Math.Cos(angle), cy + r * Math.Sin(angle)));
            }

            var element = new RealPdfElement
            {
                FillColor = NormalizeColor(circle.Attribute("fill")?.Value),
                StrokeColor = NormalizeColor(circle.Attribute("stroke")?.Value),
                StrokeWidth = ParseDouble(circle.Attribute("stroke-width")?.Value) ?? 1.0,
                Points = points,
                Type = ElementType.PlaygroundChildren
            };

            element.Properties["svg_type"] = circle.Name.LocalName;
            element.Properties["radius"] = r;
            element.Properties["area"] = Math.PI * r * r;

            return element;
        }

        private ElementType ClassifyElement(RealPdfElement element)
        {
            var scores = new Dictionary<ElementType, int>();

            foreach (var symbol in _passport.Symbols.Values)
            {
                int score = CalculateScore(element, symbol);
                if (score > 0)
                {
                    scores[symbol.Type] = score;
                }
            }
            if (scores.Count == 0)
                return ElementType.Unknown;

            return scores.OrderByDescending(s => s.Value).First().Key;
        }

        private int CalculateScore(RealPdfElement element, SymbolDefinition symbol)
        {
            int score = 0;

            if (!string.IsNullOrEmpty(element.FillColor) &&
                symbol.FillColors.Contains(element.FillColor))
                score += 10;

            if (!string.IsNullOrEmpty(element.StrokeColor) &&
                symbol.StrokeColors.Contains(element.StrokeColor))
                score += 8;

            if (!string.IsNullOrEmpty(element.StrokeDashArray) &&
                symbol.StrokeDashArrays.Contains(element.StrokeDashArray))
                score += 6;

            if (symbol.MinStrokeWidth.HasValue && symbol.MaxStrokeWidth.HasValue &&
                element.StrokeWidth >= symbol.MinStrokeWidth.Value &&
                element.StrokeWidth <= symbol.MaxStrokeWidth.Value)
                score += 4;

            if (element.Properties.ContainsKey("area") && symbol.TypicalArea.HasValue)
            {
                var area = (double)element.Properties["area"];
                var ratio = Math.Min(area, symbol.TypicalArea.Value) / Math.Max(area, symbol.TypicalArea.Value);
                if (ratio > 0.3) score += (int)(ratio * 5);
            }

            return score;
        }

        private void ExtractAllText(XDocument doc, XNamespace ns)
        {
            var texts = doc.Descendants(ns + "text");
            foreach (var text in texts)
            {
                var content = text.Value?.Trim();
                if (string.IsNullOrEmpty(content)) continue;

                var x = ParseDouble(text.Attribute("x")?.Value) ?? 0;
                var y = ParseDouble(text.Attribute("y")?.Value) ?? 0;

                _textElements.Add(new TextElement
                {
                    Content = content,
                    X = x,
                    Y = y,
                    FontSize = ParseDouble(text.Attribute("font-size")?.Value) ?? 12
                });
            }
            Console.WriteLine($"Извлечено текстовых элементов: {_textElements.Count}");
        }

        private void AssociateTextWithElements(List<RealPdfElement> elements)
        {
            foreach (var element in elements)
            {
                var center = CalculateCenter(element.Points);
                var nearbyTexts = _textElements
                    .Where(t => Distance(t.X, t.Y, center.X, center.Y) < 100)
                    .ToList();

                element.Properties["nearby_texts"] = nearbyTexts.Select(t => t.Content).ToList();

                if (nearbyTexts.Any())
                {
                    element.Properties["primary_text"] = nearbyTexts.OrderBy(t =>
                        Distance(t.X, t.Y, center.X, center.Y)).First().Content;
                }
            }
        }

        private void RefineClassification(List<RealPdfElement> elements)
        {
            foreach (var element in elements.Where(e => e.Type == ElementType.Unknown))
            {
                if (element.Properties.ContainsKey("primary_text"))
                {
                    var text = element.Properties["primary_text"].ToString().ToLower();

                    foreach (var symbol in _passport.Symbols.Values)
                    {
                        if (symbol.TextMarkers.Any(marker => text.Contains(marker.ToLower())))
                        {
                            element.Type = symbol.Type;
                            element.Properties["classified_by"] = "text_marker";
                            break;
                        }
                    }
                }

                if (element.Type == ElementType.Unknown && element.Properties.ContainsKey("area"))
                {
                    var area = (double)element.Properties["area"];
                    if (area > 1000) element.Type = ElementType.Building;
                    else if (area > 100 && area < 500) element.Type = ElementType.PlaygroundChildren;
                }
            }
        }
        private string NormalizeColor(string color)
        {
            if (string.IsNullOrEmpty(color) || color == "none") return string.Empty;
            return color.ToLower().Trim();
        }

        private double? ParseDouble(string value)
        {
            return double.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double result) ? result : null;
        }

        private List<PointD> ParsePathData(string pathData)
        {
            var points = new List<PointD>();
            if (string.IsNullOrEmpty(pathData)) return points;

            try
            {
                var commands = pathData.Split(new[] { 'M', 'L', 'Z', 'm', 'l', 'z', 'H', 'V', 'h', 'v', 'C', 'c', 'S', 's', 'Q', 'q', 'T', 't', 'A', 'a' },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var cmd in commands)
                {
                    var coords = cmd.Split(new[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < coords.Length; i += 2)
                    {
                        if (i + 1 < coords.Length &&
                            double.TryParse(coords[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double x) &&
                            double.TryParse(coords[i + 1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double y))
                        {
                            points.Add(new PointD(x, y));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка парсинга path data: {ex.Message}");
            }

            return points;
        }

        private List<PointD> ParsePointsAttribute(string pointsAttr)
        {
            var points = new List<PointD>();
            if (string.IsNullOrEmpty(pointsAttr)) return points;

            try
            {
                var coords = pointsAttr.Split(new[] { ' ', '\t', '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < coords.Length; i += 2)
                {
                    if (i + 1 < coords.Length &&
                        double.TryParse(coords[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double x) &&
                        double.TryParse(coords[i + 1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double y))
                    {
                        points.Add(new PointD(x, y));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка парсинга points attribute: {ex.Message}");
            }

            return points;
        }

        private PointD CalculateCenter(List<PointD> points)
        {
            if (points.Count == 0) return new PointD(0, 0);
            var avgX = points.Average(p => p.X);
            var avgY = points.Average(p => p.Y);
            return new PointD(avgX, avgY);
        }

        private double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        private double CalculatePathLength(List<PointD> points)
        {
            if (points.Count < 2) return 0;
            double length = 0;
            for (int i = 1; i < points.Count; i++)
            {
                length += Distance(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y);
            }
            return length;
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