using System.Collections.Generic;
using System.Threading.Tasks;
using UrbanLayoutGenerator.Models;

namespace UrbanLayoutGenerator.Services
{
    public interface IPdfParser
    {
        Task<List<SiteElement>> ParseAsync(string pdfPath);
    }
}