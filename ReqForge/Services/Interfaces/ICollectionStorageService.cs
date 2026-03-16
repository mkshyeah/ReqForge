using ReqForge.Models;

namespace ReqForge.Services.Interfaces;

public interface ICollectionStorageService
{
    List<RequestCollection> LoadAll();
    
    void SaveAll(List<RequestCollection> collections);
}