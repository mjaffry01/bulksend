using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace bulksend
{
    public static class JsonHelper
    {
        public static T LoadFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"JSON file not found: {filePath}");
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<T>(jsonContent);

                if (data == null)
                {
                    throw new InvalidOperationException("Failed to parse JSON data.");
                }

                return data;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Error parsing JSON: {ex.Message}");
            }
        }
    }
}
