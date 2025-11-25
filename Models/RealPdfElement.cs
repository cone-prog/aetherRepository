using System;
using System.Collections.Generic;

namespace UrbanLayoutGenerator.Models
{
    public enum ElementType
    {
        Unknown = 0,
        SiteBoundary,
        RedLine,
        LandscapingBoundary,
        Building,
        UndergroundFloor,
        PlaygroundChildren,
        PlaygroundSports,
        PlaygroundAdults,
        SocialKindergarten,
        SocialSchool
    }

    public class RealPdfElement
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ElementType Type { get; set; }
        public List<PointD> Points { get; set; } = new();
        public string FillColor { get; set; } = string.Empty;
        public string StrokeColor { get; set; } = string.Empty;
        public double StrokeWidth { get; set; }
        public string StrokeDashArray { get; set; } = string.Empty;
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class PointD
    {
        public double X { get; set; }
        public double Y { get; set; }

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class TextElement
    {
        public string Content { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double FontSize { get; set; }
    }

    public class ParsingResult
    {
        public List<RealPdfElement> Elements { get; set; } = new();
        public List<TextElement> TextElements { get; set; } = new();
        public int UnclassifiedCount { get; set; }
        public int TotalElements => Elements.Count;
        public int ClassifiedCount => TotalElements - UnclassifiedCount;
    }
}