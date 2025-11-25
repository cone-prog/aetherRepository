
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

    public class SiteElement
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ElementType Type { get; set; }
        public List<GeometryPoint> Points { get; set; } = new();
        public string OriginalColor { get; set; } = string.Empty;
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class GeometryPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public GeometryPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}