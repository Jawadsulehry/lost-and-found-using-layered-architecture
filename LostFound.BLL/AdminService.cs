using LostFound.DAL;
using LostFound.Models;

namespace LostFound.BLL;

public class AdminService
{
    private readonly IUserRepository _users;
    private readonly INotificationRepository _notes;

    public AdminService() : this(new UserRepository(), new NotificationRepository()) { }

    public AdminService(IUserRepository users, INotificationRepository notes)
    {
        _users = users;
        _notes = notes;
        Database.Initialize();
    }

    // ----- Account requests -----
    public List<User> GetPendingRequests() => _users.GetByStatus(AccountStatus.Pending);

    public void ApproveAccount(int userId) => Decide(userId, AccountStatus.Approved);
    public void RejectAccount(int userId) => Decide(userId, AccountStatus.Rejected);

    private void Decide(int userId, AccountStatus decision)
    {
        var user = _users.GetById(userId)
            ?? throw new InvalidOperationException("Account request not found.");
        if (user.Status != AccountStatus.Pending)
            throw new InvalidOperationException("This request was already handled.");

        _users.UpdateStatus(userId, decision);

        // The request notification is finished, so mark it as read
        foreach (var n in _notes.GetForUser(0)
                     .Where(n => n.Type == "AccountRequest" && n.RelatedId == userId && !n.IsRead))
            _notes.MarkRead(n.Id);
    }

    // ----- Admin notification inbox (RecipientUserId 0) -----
    public List<Notification> GetInbox() => _notes.GetForUser(0);

    public void MarkRead(int notificationId) => _notes.MarkRead(notificationId);

    public void MarkAllRead()
    {
        foreach (var n in _notes.GetForUser(0).Where(n => !n.IsRead))
            _notes.MarkRead(n.Id);
    }
}