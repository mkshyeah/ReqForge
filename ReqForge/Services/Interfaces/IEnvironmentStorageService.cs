using ReqForge.Models.DTOs;

namespace ReqForge.Services.Interfaces;

public interface IEnvironmentStorageService
{
    List<RequestEnvironmentDto> LoadAll(string username);
    void Add(RequestEnvironmentDto environment, string username);
    void Update(RequestEnvironmentDto environment);
    void Delete(int environmentId);
    void SaveAll(List<RequestEnvironmentDto> environments, string username);
}
