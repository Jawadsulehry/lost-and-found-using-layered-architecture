using System.Security.Cryptography;
using System.Text;
using LostFound.DAL;
using LostFound.Models;

namespace LostFound.BLL;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly INotificationRepository _notes;

    public AuthService() : this(new UserRepository(), new NotificationRepository()) { }

    public AuthService(IUserRepository users, INotificationRepository notes)
    {
        _users = users;
        _notes = notes;
        Database.Initialize();
        EnsureAdmin();
    }

    // Creates the default admin the first time the app runs
    private void EnsureAdmin()
    {
        if (_users.GetByUsername("admin") != null) return;
        _users.Add(new User
        {
            FullName = "Administrator",
            Username = "admin",
            PasswordHash = Hash("admin", "admin123"),
            Role = UserRole.Admin,
            Status = AccountStatus.Approved
        });
    }

    // Passwords are never stored as plain text
    public static string Hash(string username, string password) =>
        Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(username.ToLower() + ":" + password)));

    public void Register(string fullName, string username, string password, string confirm)
    {
        fullName = fullName.Trim();
        username = username.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.");
        if (username.Length < 3 || username.Contains(' '))
            throw new ArgumentException("Username must be at least 3 characters with no spaces.");
        if (password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters.");
        if (password != confirm)
            throw new ArgumentException("Passwords do not match.");
        if (_users.GetByUsername(username) != null)
            throw new ArgumentException("That username is already taken.");

        int id = _users.Add(new User
        {
            FullName = fullName,
            Username = username,
            PasswordHash = Hash(username, password),
            Role = UserRole.Member,
            Status = AccountStatus.Pending
        });

        // RecipientUserId 0 = Admin inbox
        _notes.Add(new Notification
        {
            RecipientUserId = 0,
            Type = "AccountRequest",
            Message = $"{fullName} ({username}) requested an account.",
            RelatedId = id
        });
    }

    public User Login(string username, string password)
    {
        username = username.Trim().ToLower();
        var user = _users.GetByUsername(username);

        if (user == null || user.PasswordHash != Hash(username, password))
            throw new InvalidOperationException("Invalid username or password.");
        if (user.Status == AccountStatus.Pending)
            throw new InvalidOperationException("Your account is waiting for admin approval.");
        if (user.Status == AccountStatus.Rejected)
            throw new InvalidOperationException("Your account request was rejected.");

        return user;
    }
}