using System.Collections.Generic;

namespace UrbanLayoutGenerator.Core.Models;
public class ParsingResult
{
    public List<RealPdfElement> Elements { get; set; } = new();
    public List<TextElement> TextElements { get; set; } = new();
    public int UnclassifiedCount { get; set; }
    public int TotalElements => Elements.Count;
    public int ClassifiedCount => TotalElements - UnclassifiedCount;
}