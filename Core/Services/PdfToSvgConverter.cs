using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace UrbanLayoutGenerator.Services
{
    public class PdfToSvgConverter
    {
        public async Task<string> ConvertToSvgAsync(string pdfPath, string outputDir = null)
        {
            outputDir ??= Path.GetDirectoryName(pdfPath) ?? ".";
            var svgPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(pdfPath) + ".svg");

            var processInfo = new ProcessStartInfo
            {
                FileName = "pdftocairo",
                Arguments = $"-svg \"{pdfPath}\" \"{svgPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
                throw new InvalidOperationException("Не удалось запустить pdftocairo");

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new Exception($"pdftocairo error: {error}");
            }

            Console.WriteLine($"Конвертировано: {pdfPath} -> {svgPath}");
            return svgPath;
        }

        public async Task<List<string>> ExtractImagesAsync(string pdfPath, string outputDir)
        {
            var images = new List<string>();

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var processInfo = new ProcessStartInfo
            {
                FileName = "pdfimages",
                Arguments = $"-all \"{pdfPath}\" \"{Path.Combine(outputDir, "image")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();

                foreach (var file in Directory.GetFiles(outputDir, "image"))
                {
                    images.Add(file);
                }
            }

            return images;
        }
    }
}