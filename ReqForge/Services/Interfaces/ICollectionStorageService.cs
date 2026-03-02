using ReqForge.Models;

namespace ReqForge.Services;

public interface ICollectionStorageService
{
    List<RequestCollection> LoadAll();
    
    void SaveAll(List<RequestCollection> collections);
}