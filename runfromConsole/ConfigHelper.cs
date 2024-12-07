using System.IO;
using System.Text.Json;

public static class ConfigHelper
{
    public static T LoadFromJsonFile<T>(string fileName)
    {
        string basePath = AppContext.BaseDirectory; // Get execution directory
        string filePath = Path.Combine(basePath, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        try
        {
            string jsonContent = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<T>(jsonContent);

            if (data == null)
            {
                throw new InvalidOperationException($"Failed to parse {fileName}");
            }

            return data;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Error parsing {fileName}: {ex.Message}");
        }
    }
}
