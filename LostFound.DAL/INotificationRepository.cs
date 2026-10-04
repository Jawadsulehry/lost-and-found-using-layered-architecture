using LostFound.Models;

namespace LostFound.DAL;

public interface INotificationRepository
{
    void Add(Notification n);
    List<Notification> GetForUser(int userId);   // userId 0 = Admin inbox
    void MarkRead(int id);
    int CountUnread(int userId);
}