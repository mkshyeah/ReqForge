using System.IO;
using System.Text.Json;
using ReqForge.Models;
using ReqForge.Services.Interfaces;


namespace ReqForge.Services;

public class CollectionStorageService : ICollectionStorageService
{
    private static readonly string _filePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "collections.json");

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };
    
    public List<RequestCollection> LoadAll()
    {
        if (!File.Exists(_filePath))
        {
            return new List<RequestCollection>();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<RequestCollection>>(json) ?? new List<RequestCollection>();
        }
        catch
        {
            return new List<RequestCollection>();
        }
    }

    public void SaveAll(List<RequestCollection> collections)
    {
        var json = JsonSerializer.Serialize(collections, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }
}