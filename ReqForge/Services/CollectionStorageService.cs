using Microsoft.EntityFrameworkCore;
using ReqForge.Data;
using ReqForge.Models;
using ReqForge.Services.Interfaces;

namespace ReqForge.Services;

public class CollectionStorageService : ICollectionStorageService
{
    private readonly AppDbContext _db;

    public CollectionStorageService(AppDbContext db)
    {
        _db = db;
    }

    public List<RequestCollection> LoadAll(string username)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserName == username);
        if (user == null) return new List<RequestCollection>();

        var collections = _db.Collections
            .Where(c => c.UserId == user.Id)
            .Include(c => c.Requests)
                .ThenInclude(r => r.Headers)
            .Include(c => c.Requests)
                .ThenInclude(r => r.QueryParams)
            .AsNoTracking()
            .ToList();

        foreach (var collection in collections)
        foreach (var request in collection.Requests)
            DecryptRequestSecrets(request);

        return collections;
    }

    public void AddCollection(RequestCollection collection, string username)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserName == username);
        if (user == null) return;

        collection.UserId = user.Id;
        _db.Collections.Add(collection);
        _db.SaveChanges();
    }

    public void UpdateCollection(RequestCollection collection)
    {
        var existing = _db.Collections.Find(collection.Id);
        if (existing == null) return;

        existing.Name = collection.Name;
        _db.SaveChanges();
    }

    public void DeleteCollection(int collectionId)
    {
        var collection = _db.Collections.Find(collectionId);
        if (collection == null) return;

        _db.Collections.Remove(collection);
        _db.SaveChanges();
    }

    public void AddRequest(SavedRequest request, int collectionId)
    {
        EncryptRequestSecrets(request);
        request.CollectionId = collectionId;
        _db.SavedRequests.Add(request);
        _db.SaveChanges();
        DecryptRequestSecrets(request);
    }

    public void UpdateRequest(SavedRequest request)
    {
        var existing = _db.SavedRequests
            .Include(r => r.Headers)
            .Include(r => r.QueryParams)
            .FirstOrDefault(r => r.Id == request.Id);

        if (existing == null) return;

        existing.Name = request.Name;
        existing.Url = request.Url;
        existing.Method = request.Method;
        existing.Body = request.Body;
        existing.BodyType = request.BodyType;
        existing.AuthType = request.AuthType;
        existing.BearerToken = SecretProtector.Protect(request.BearerToken);
        existing.BasicAuthUsername = request.BasicAuthUsername;
        existing.BasicAuthPassword = SecretProtector.Protect(request.BasicAuthPassword);
        existing.ApiKeyName = request.ApiKeyName;
        existing.ApiKeyValue = SecretProtector.Protect(request.ApiKeyValue);

        _db.RequestHeaders.RemoveRange(existing.Headers);
        existing.Headers = request.Headers;

        _db.QueryParams.RemoveRange(existing.QueryParams);
        existing.QueryParams = request.QueryParams;

        _db.SaveChanges();
    }

    public void DeleteRequest(int requestId)
    {
        var request = _db.SavedRequests.Find(requestId);
        if (request == null) return;

        _db.SavedRequests.Remove(request);
        _db.SaveChanges();
    }

    public void SaveAll(List<RequestCollection> collections, string username)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserName == username);
        if (user == null) return;

        var existing = _db.Collections
            .Where(c => c.UserId == user.Id)
            .ToList();
        _db.Collections.RemoveRange(existing);

        foreach (var col in collections)
        {
            col.UserId = user.Id;
            col.Id = 0;
            foreach (var req in col.Requests)
            {
                EncryptRequestSecrets(req);
                req.Id = 0;
                req.CollectionId = 0;
                foreach (var h in req.Headers)
                {
                    h.Id = 0;
                    h.SavedRequestId = 0;
                }
                foreach (var q in req.QueryParams)
                {
                    q.Id = 0;
                    q.SavedRequestId = 0;
                }
            }
        }

        _db.Collections.AddRange(collections);
        _db.SaveChanges();

        foreach (var col in collections)
        foreach (var req in col.Requests)
            DecryptRequestSecrets(req);
    }

    private static void EncryptRequestSecrets(SavedRequest request)
    {
        request.BearerToken = SecretProtector.Protect(request.BearerToken);
        request.BasicAuthPassword = SecretProtector.Protect(request.BasicAuthPassword);
        request.ApiKeyValue = SecretProtector.Protect(request.ApiKeyValue);
    }

    private static void DecryptRequestSecrets(SavedRequest request)
    {
        request.BearerToken = SecretProtector.Unprotect(request.BearerToken);
        request.BasicAuthPassword = SecretProtector.Unprotect(request.BasicAuthPassword);
        request.ApiKeyValue = SecretProtector.Unprotect(request.ApiKeyValue);
    }
}
