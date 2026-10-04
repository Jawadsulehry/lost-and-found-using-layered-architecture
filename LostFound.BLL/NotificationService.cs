using LostFound.DAL;
using LostFound.Models;

namespace LostFound.BLL;

public class NotificationService
{
    private readonly INotificationRepository _notes;

    public NotificationService() : this(new NotificationRepository()) { }

    public NotificationService(INotificationRepository notes)
    {
        _notes = notes;
        Database.Initialize();
    }

    public List<Notification> GetFor(int userId) => _notes.GetForUser(userId);

    public void MarkRead(int id) => _notes.MarkRead(id);

    public void MarkAllRead(int userId)
    {
        foreach (var n in _notes.GetForUser(userId).Where(n => !n.IsRead))
            _notes.MarkRead(n.Id);
    }
}