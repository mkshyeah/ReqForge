using ReqForge.Models.DTOs;

namespace ReqForge.Services.Interfaces;

public interface IEnvironmentStorageService
{
    List<RequestEnvironmentDto> LoadAll(string username);
    void SaveAll(List<RequestEnvironmentDto> environments, string username);
}