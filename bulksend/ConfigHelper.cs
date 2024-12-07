using System;
using System.IO;
using System.Text.Json;

namespace bulksend
{
    public static class ConfigHelper
    {
        public static T LoadFromJsonFile<T>(string fileName)
        {
            // Resolve the full file path using AppContext.BaseDirectory
            string basePath = AppContext.BaseDirectory;
            string filePath = Path.Combine(basePath, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(jsonContent)
                       ?? throw new InvalidOperationException("Failed to deserialize JSON data.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Error parsing JSON file {fileName}: {ex.Message}");
            }
        }
    }
}
