using ReqForge.Models;

namespace ReqForge.Services.Interfaces;

public interface ICollectionStorageService
{
    List<RequestCollection> LoadAll(string username);
    
    void SaveAll(List<RequestCollection> collections, string username);
}