public class ProcessingResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OriginalFileName { get; set; } = string.Empty;
    public string GeoJsonPath { get; set; } = string.Empty;
    public string PassportPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int TotalElements { get; set; }
    public int ClassifiedCount { get; set; }
    public string Status { get; set; } = "completed";
}