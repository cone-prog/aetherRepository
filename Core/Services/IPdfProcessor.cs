using System.Threading.Tasks;
using UrbanLayoutGenerator.Core.Models;

namespace UrbanLayoutGenerator.Core.Services
{
    public interface IPdfProcessor
    {
        Task<ParsingResult> ProcessPdfAsync(string pdfPath);
        Task<string> ConvertToSvgAsync(string pdfPath);
    }
}