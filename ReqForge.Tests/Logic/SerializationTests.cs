using System.Collections.ObjectModel;
using System.Text.Json;
using ReqForge.Models;

namespace ReqForge.Tests.Logic;

public class SerializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static RequestCollection MakeCollection(string name = "Test Collection")
    {
        return new RequestCollection
        {
            Id = 1,
            Name = name,
            UserId = 1,
            Requests = new ObservableCollection<SavedRequest>
            {
                new()
                {
                    Id = 1,
                    CollectionId = 1,
                    Name = "Get Users",
                    Url = "https://api.example.com/users",
                    Method = "GET",
                    Body = "",
                    BodyType = "none",
                    AuthType = "Bearer Token",
                    BearerToken = "abc123"
                }
            }
        };
    }

    [Fact]
    public void Serialize_Collection_ProducesValidJson()
    {
        var collections = new List<RequestCollection> { MakeCollection() };

        var json = JsonSerializer.Serialize(collections, JsonOptions);

        Assert.NotNull(json);
        Assert.NotEmpty(json);

        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    [Fact]
    public void Roundtrip_CollectionPreservesData()
    {
        var original = new List<RequestCollection> { MakeCollection() };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<List<RequestCollection>>(json);

        Assert.NotNull(restored);
        Assert.Single(restored);
        Assert.Equal("Test Collection", restored[0].Name);
        Assert.Single(restored[0].Requests);

        var req = restored[0].Requests[0];
        Assert.Equal("Get Users", req.Name);
        Assert.Equal("https://api.example.com/users", req.Url);
        Assert.Equal("GET", req.Method);
        Assert.Equal("Bearer Token", req.AuthType);
        Assert.Equal("abc123", req.BearerToken);
    }

    [Fact]
    public void Roundtrip_MultipleCollections()
    {
        var original = new List<RequestCollection>
        {
            MakeCollection("API v1"),
            MakeCollection("API v2")
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<List<RequestCollection>>(json);

        Assert.NotNull(restored);
        Assert.Equal(2, restored.Count);
        Assert.Equal("API v1", restored[0].Name);
        Assert.Equal("API v2", restored[1].Name);
    }

    [Fact]
    public void Roundtrip_EmptyCollections()
    {
        var original = new List<RequestCollection>();

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<List<RequestCollection>>(json);

        Assert.NotNull(restored);
        Assert.Empty(restored);
    }

    [Fact]
    public void Roundtrip_RequestWithAllAuthTypes()
    {
        var collection = new RequestCollection
        {
            Name = "Auth Tests",
            Requests = new ObservableCollection<SavedRequest>
            {
                new()
                {
                    Name = "Bearer",
                    Url = "https://api.test.com",
                    Method = "POST",
                    AuthType = "Bearer Token",
                    BearerToken = "my-token"
                },
                new()
                {
                    Name = "Basic",
                    Url = "https://api.test.com",
                    Method = "GET",
                    AuthType = "Basic Auth",
                    BasicAuthUsername = "admin",
                    BasicAuthPassword = "secret"
                },
                new()
                {
                    Name = "API Key",
                    Url = "https://api.test.com",
                    Method = "GET",
                    AuthType = "API Key",
                    ApiKeyName = "X-API-Key",
                    ApiKeyValue = "key-123"
                }
            }
        };

        var json = JsonSerializer.Serialize(new List<RequestCollection> { collection }, JsonOptions);
        var restored = JsonSerializer.Deserialize<List<RequestCollection>>(json);

        Assert.NotNull(restored);
        var requests = restored[0].Requests;

        Assert.Equal("my-token", requests[0].BearerToken);
        Assert.Equal("admin", requests[1].BasicAuthUsername);
        Assert.Equal("secret", requests[1].BasicAuthPassword);
        Assert.Equal("X-API-Key", requests[2].ApiKeyName);
        Assert.Equal("key-123", requests[2].ApiKeyValue);
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsException()
    {
        var invalidJson = "{ not valid json }}}";

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<List<RequestCollection>>(invalidJson));
    }

    [Fact]
    public void Serialize_UnicodeCharacters_PreservedCorrectly()
    {
        var collection = new RequestCollection
        {
            Name = "Тестовая коллекция",
            Requests = new ObservableCollection<SavedRequest>
            {
                new()
                {
                    Name = "Запрос пользователей",
                    Url = "https://api.example.com/пользователи",
                    Method = "GET"
                }
            }
        };

        var json = JsonSerializer.Serialize(new List<RequestCollection> { collection }, JsonOptions);

        Assert.Contains("Тестовая коллекция", json);
        Assert.Contains("Запрос пользователей", json);

        var restored = JsonSerializer.Deserialize<List<RequestCollection>>(json);
        Assert.Equal("Тестовая коллекция", restored![0].Name);
    }
}
