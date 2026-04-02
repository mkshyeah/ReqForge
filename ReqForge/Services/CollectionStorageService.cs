using System.IO;
using System.Text.Json;
using ReqForge.Models;
using ReqForge.Services.Interfaces;


namespace ReqForge.Services;

public class CollectionStorageService : ICollectionStorageService{

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private static string GetFilePath(string username)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"collections_{username}.json");
    }
    
    public List<RequestCollection> LoadAll(string username)
    {
        var filePath = GetFilePath(username);
        if (!File.Exists(filePath))
        {
            return new List<RequestCollection>();
        }

        try
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<RequestCollection>>(json) ?? new List<RequestCollection>();
        }
        
        catch
        {
            return new List<RequestCollection>();
        }
    }

    public void SaveAll(List<RequestCollection> collections, string username)
    {
        var filePath = GetFilePath(username);
        var json = JsonSerializer.Serialize(collections, _jsonOptions);
        File.WriteAllText(filePath, json);
    }
}