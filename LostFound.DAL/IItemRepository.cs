using LostFound.Models;

namespace LostFound.DAL;

public interface IItemRepository
{
    int Add(Item item);
    List<Item> GetAll();
    Item? GetById(int id);
    void UpdateStatus(int id, ItemStatus status);
    void SetMatch(int id, ItemStatus status, int matchedItemId, string pickupLocation);
    void Delete(int id);
}