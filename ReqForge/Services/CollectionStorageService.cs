using System.IO;
using System.Text.Json;
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
        if(user==null) return new List<RequestCollection>();


        return _db.Collections
            .Where(c => c.UserId == user.Id)
            .Include(c => c.Requests)
            .ThenInclude(r => r.Headers)
            .ToList();
    }

    public void SaveAll(List<RequestCollection> collections, string username)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserName == username);
        if(user==null) return;
        
        // Удаляем старые коллекции этого юзера
        var existing = _db.Collections
            .Where(c => c.UserId == user.Id)
            .ToList();
        _db.Collections.RemoveRange(existing);
        
        // Добавляем текущие
        foreach (var col in collections)
        {
            col.UserId = user.Id;
            col.Id = 0; // сброс, чтоб EF создал новые записи
            foreach (var req in col.Requests)
            {
                req.Id = 0;
                req.CollectionId = 0;
                foreach (var h in req.Headers)
                {
                    h.Id = 0;
                    h.SavedRequestId = 0;
                }
            }
        }
        _db.Collections.AddRange(collections);
        _db.SaveChanges();
    }
}