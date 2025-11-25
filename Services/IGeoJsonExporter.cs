using System.Collections.Generic;
using System.Threading.Tasks;
using UrbanLayoutGenerator.Models;

namespace UrbanLayoutGenerator.Services
{
    public interface IGeoJsonExporter
    {
        Task<string> ExportAsync(List<SiteElement> elements, string outputPath);
    }
}