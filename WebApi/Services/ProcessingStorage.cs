using System.Collections.Concurrent;

namespace UrbanLayoutGenerator.WebApi.Services
{
    public interface IProcessingStorage
    {
        string StoreResult(ProcessingResult result);
        ProcessingResult? GetResult(string id);
        bool DeleteResult(string id);
    }

    public class ProcessingStorage : IProcessingStorage
    {
        private readonly ConcurrentDictionary<string, ProcessingResult> _storage = new();
        private readonly string _storagePath;

        public ProcessingStorage()
        {
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "ProcessingResults");
            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);
        }

        public string StoreResult(ProcessingResult result)
        {
            _storage[result.Id] = result;

            var metaPath = Path.Combine(_storagePath, $"{result.Id}.json");
            File.WriteAllText(metaPath, System.Text.Json.JsonSerializer.Serialize(result));

            return result.Id;
        }

        public ProcessingResult? GetResult(string id)
        {
            if (_storage.TryGetValue(id, out var result))
                return result;

            var metaPath = Path.Combine(_storagePath, $"{id}.json");
            if (File.Exists(metaPath))
            {
                var json = File.ReadAllText(metaPath);
                return System.Text.Json.JsonSerializer.Deserialize<ProcessingResult>(json);
            }

            return null;
        }

        public bool DeleteResult(string id)
        {
            _storage.TryRemove(id, out _);

            var metaPath = Path.Combine(_storagePath, $"{id}.json");
            if (File.Exists(metaPath))
                File.Delete(metaPath);

            return true;
        }
    }
}