using ReqForge.Models;

namespace ReqForge.Services.Interfaces;

public interface ICollectionStorageService
{
    List<RequestCollection> LoadAll(string username);
    void AddCollection(RequestCollection collection, string username);
    void UpdateCollection(RequestCollection collection);
    void DeleteCollection(int collectionId);
    void AddRequest(SavedRequest request, int collectionId);
    void UpdateRequest(SavedRequest request);
    void DeleteRequest(int requestId);
    void SaveAll(List<RequestCollection> collections, string username);
}
