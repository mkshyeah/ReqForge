using ReqForge.Models.DTOs;

namespace ReqForge.Services.Interfaces;

public interface IEnvironmentStorageService
{
    List<RequestEnvironmentDto> LoadAll();
    void SaveAll(List<RequestEnvironmentDto> environments);
}