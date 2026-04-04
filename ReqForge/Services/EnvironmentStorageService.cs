using Microsoft.EntityFrameworkCore;
using ReqForge.Data;
using ReqForge.Models.DTOs;
using ReqForge.Services.Interfaces;

namespace ReqForge.Services;

public class EnvironmentStorageService : IEnvironmentStorageService
{
    private readonly AppDbContext _db;

    public EnvironmentStorageService(AppDbContext db)
    {
        _db = db;
    }

    public List<RequestEnvironmentDto> LoadAll(string username)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserName == username);
        if (user == null) return new List<RequestEnvironmentDto>();

        return _db.Environments
            .Where(e => e.UserId == user.Id)
            .Include(e => e.Variables)
            .ToList();
    }

    public void SaveAll(List<RequestEnvironmentDto> environments, string username)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserName == username);
        if (user == null) return;

        var existing = _db.Environments
            .Where(e => e.UserId == user.Id)
            .ToList();
        _db.Environments.RemoveRange(existing);

        foreach (var env in environments)
        {
            env.UserId = user.Id;
            env.Id = 0;
            foreach (var v in env.Variables)
            {
                v.Id = 0;
                v.EnvironmentId = 0;
            }
        }
        _db.Environments.AddRange(environments);
        _db.SaveChanges();
    }
}