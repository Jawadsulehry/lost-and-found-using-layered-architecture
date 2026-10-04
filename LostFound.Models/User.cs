namespace LostFound.Models;

public enum UserRole { Admin, Member }
public enum AccountStatus { Pending, Approved, Rejected }

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.Member;
    public AccountStatus Status { get; set; } = AccountStatus.Pending;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}