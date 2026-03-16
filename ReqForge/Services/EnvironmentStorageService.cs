using System.IO;
using System.Text.Json;
using ReqForge.Models;
using ReqForge.Models.DTOs;
using ReqForge.Services.Interfaces;

namespace ReqForge.Services;

public class EnvironmentStorageService : IEnvironmentStorageService
{
    private static readonly string _filePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "environments.json");
    
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };
    
    public List<RequestEnvironmentDto> LoadAll()
    {
        if (!File.Exists(_filePath))
        {
            return new List<RequestEnvironmentDto>();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<RequestEnvironmentDto>>(json) ?? new List<RequestEnvironmentDto>();
        }
        catch
        {
            return new List<RequestEnvironmentDto>();
        }
    }

    public void SaveAll(List<RequestEnvironmentDto> environments)
    {
        var json = JsonSerializer.Serialize(environments, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }
}