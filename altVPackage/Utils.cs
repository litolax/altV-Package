using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ImprovedConsole;

namespace altVPackage;

public static class Utils
{
    public static async Task DownloadFile(string url, string fileName)
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        byte[] fileBytes;
        try
        {
            fileBytes = await httpClient.GetByteArrayAsync(url);
        }
        catch
        {
            ConsoleWrapper.WriteLine($"Connection timeout on {fileName}", LogType.Error, true);
            Console.ReadLine();
            return;
        }
        await File.WriteAllBytesAsync(fileName, fileBytes);
    }

    public static async Task<string> GetUpdatedHash(string url, string file)
    {
        var httpClient = new HttpClient();
        var fileBytes = await httpClient.GetByteArrayAsync(url);
        var json = Encoding.UTF8.GetString(fileBytes, 0, fileBytes.Length);
        return json.Substring(json.IndexOf($"{file}", StringComparison.Ordinal) + file.Length + 3, 40);
    }

    public static async Task<string> CalculateHash(string filePath)
    {
        if (!File.Exists(filePath)) return string.Empty;
        await using var fileStream = File.OpenRead($"{filePath}");
        {
            return Convert.ToHexString(await SHA1.Create().ComputeHashAsync(fileStream)).ToLower();
        }
    }
    
    public static async Task<string?> GetVersion(string branch, string system)
    {
        var httpClient = new HttpClient();
        var fileBytes = await httpClient.GetByteArrayAsync($"https://cdn.alt-mp.com/server/{branch}/{system}/update.json");
        
        var json = Encoding.UTF8.GetString(fileBytes, 0, fileBytes.Length);
        
        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            return doc.RootElement.GetProperty("version").GetString();
        }
    }
}