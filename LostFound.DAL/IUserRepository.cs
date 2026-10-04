using LostFound.Models;

namespace LostFound.DAL;

public interface IUserRepository
{
    int Add(User user);
    User? GetByUsername(string username);
    User? GetById(int id);
    List<User> GetByStatus(AccountStatus status);
    void UpdateStatus(int id, AccountStatus status);
}