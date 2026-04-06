using ReqForge.Data;
using ReqForge.Models;
using ReqForge.Services.Interfaces;

namespace ReqForge.Services;

public class RequestHistoryService : IRequestHistoryService
{
    private readonly AppDbContext _db;

    public RequestHistoryService(AppDbContext db)
    {
        _db = db;
    }

    public List<RequestHistoryItem> LoadByUser(string username)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserName == username);
        if (user == null) return new List<RequestHistoryItem>();

        return _db.RequestHistory
            .Where(h => h.UserId == user.Id)
            .OrderByDescending(h => h.SentAt)
            .ToList();
    }

    public void Add(RequestHistoryItem item, string username)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserName == username);
        if (user == null) return;

        item.UserId = user.Id;
        _db.RequestHistory.Add(item);
        _db.SaveChanges();
    }

    public void ClearByUser(string username)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserName == username);
        if (user == null) return;

        var items = _db.RequestHistory.Where(h => h.UserId == user.Id).ToList();
        _db.RequestHistory.RemoveRange(items);
        _db.SaveChanges();
    }
}
