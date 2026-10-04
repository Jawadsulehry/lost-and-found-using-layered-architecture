using LostFound.DAL;
using LostFound.Models;

namespace LostFound.BLL;

public class ItemService
{
    private readonly IItemRepository _repo;
    private readonly INotificationRepository _notes;

    public ItemService() : this(new ItemRepository(), new NotificationRepository()) { }

    public ItemService(IItemRepository repo, INotificationRepository notes)
    {
        _repo = repo;
        _notes = notes;
        Database.Initialize();
    }

    public void Report(Item item)
    {
        item.Name = item.Name.Trim();
        item.Location = item.Location.Trim();
        item.ReporterContact = item.ReporterContact.Trim();

        if (item.ReporterId <= 0)
            throw new ArgumentException("You must be logged in to report an item.");
        if (string.IsNullOrWhiteSpace(item.Name))
            throw new ArgumentException("Item name is required.");
        if (string.IsNullOrWhiteSpace(item.Location))
            throw new ArgumentException("Location is required.");
        if (string.IsNullOrWhiteSpace(item.ReporterContact))
            throw new ArgumentException("Contact (phone or email) is required.");
        if (item.Date.Date > DateTime.Today)
            throw new ArgumentException("Date cannot be in the future.");
        if (item.Status != ItemStatus.Lost && item.Status != ItemStatus.Found)
            throw new ArgumentException("A new report must be Lost or Found.");

        int id = _repo.Add(item);

        // The admin is told about every new Lost or Found report (RecipientUserId 0 = Admin)
        _notes.Add(new Notification
        {
            RecipientUserId = 0,
            Type = item.Status == ItemStatus.Lost ? "NewLostReport" : "NewFoundReport",
            Message = $"{item.ReporterName} reported a {item.Status.ToString().ToUpper()} item: " +
                      $"{item.Name} ({item.Location}).",
            RelatedId = id
        });
    }

    public List<Item> GetAll() => _repo.GetAll();
    public Item? GetById(int id) => _repo.GetById(id);
    public void Delete(int id) => _repo.Delete(id);

    public List<Item> GetByReporter(int userId) =>
        _repo.GetAll().Where(i => i.ReporterId == userId).ToList();

    public List<Item> Search(string? keyword, string? category, ItemStatus? status,
                             DateTime? from, DateTime? to)
    {
        IEnumerable<Item> items = _repo.GetAll();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim().ToLower();
            items = items.Where(i => i.Name.ToLower().Contains(k)
                || i.Description.ToLower().Contains(k)
                || i.Location.ToLower().Contains(k));
        }
        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            items = items.Where(i => i.Category == category);
        if (status.HasValue)
            items = items.Where(i => i.Status == status.Value);
        if (from.HasValue)
            items = items.Where(i => i.Date.Date >= from.Value.Date);
        if (to.HasValue)
            items = items.Where(i => i.Date.Date <= to.Value.Date);

        return items.ToList();
    }
}