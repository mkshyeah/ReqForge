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
            .AsNoTracking()
            .ToList();
    }

    public void Add(RequestEnvironmentDto environment, string username)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserName == username);
        if (user == null) return;

        environment.UserId = user.Id;
        _db.Environments.Add(environment);
        _db.SaveChanges();
    }

    public void Update(RequestEnvironmentDto environment)
    {
        var existing = _db.Environments
            .Include(e => e.Variables)
            .FirstOrDefault(e => e.Id == environment.Id);

        if (existing == null) return;

        existing.Name = environment.Name;

        _db.EnvironmentVariables.RemoveRange(existing.Variables);
        existing.Variables = environment.Variables;

        _db.SaveChanges();
    }

    public void Delete(int environmentId)
    {
        var env = _db.Environments.Find(environmentId);
        if (env == null) return;

        _db.Environments.Remove(env);
        _db.SaveChanges();
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
