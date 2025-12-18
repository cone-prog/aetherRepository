using System;
using System.Collections.Generic;

namespace UrbanLayoutGenerator.Core.Models;
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