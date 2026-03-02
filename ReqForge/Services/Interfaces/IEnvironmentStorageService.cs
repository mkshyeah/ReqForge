using ReqForge.Models;

namespace ReqForge.Services;

public interface IEnvironmentStorageService
{
    List<RequestEnvironmentDto> LoadAll();
    void SaveAll(List<RequestEnvironmentDto> environments);
}