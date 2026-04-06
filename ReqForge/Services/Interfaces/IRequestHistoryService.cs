using ReqForge.Models;

namespace ReqForge.Services.Interfaces;

public interface IRequestHistoryService
{
    List<RequestHistoryItem> LoadByUser(string username);
    void Add(RequestHistoryItem item, string username);
    void ClearByUser(string username);
}
