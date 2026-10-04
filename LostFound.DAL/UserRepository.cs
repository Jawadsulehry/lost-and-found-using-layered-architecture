using LostFound.Models;
using Microsoft.Data.Sqlite;

namespace LostFound.DAL;

public class UserRepository : IUserRepository
{
    private const string Columns = "Id, FullName, Username, PasswordHash, Role, Status, CreatedDate";

    public int Add(User user)
    {
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Users (FullName, Username, PasswordHash, Role, Status, CreatedDate)
                            VALUES ($f, $u, $p, $r, $s, $d)";
        cmd.Parameters.AddWithValue("$f", user.FullName);
        cmd.Parameters.AddWithValue("$u", user.Username);
        cmd.Parameters.AddWithValue("$p", user.PasswordHash);
        cmd.Parameters.AddWithValue("$r", user.Role.ToString());
        cmd.Parameters.AddWithValue("$s", user.Status.ToString());
        cmd.Parameters.AddWithValue("$d", user.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.ExecuteNonQuery();

        var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid()";
        return Convert.ToInt32(idCmd.ExecuteScalar());
    }

    public User? GetByUsername(string username)
    {
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {Columns} FROM Users WHERE Username = $u";
        cmd.Parameters.AddWithValue("$u", username);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Map(r) : null;
    }

    public User? GetById(int id)
    {
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {Columns} FROM Users WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Map(r) : null;
    }

    public List<User> GetByStatus(AccountStatus status)
    {
        var list = new List<User>();
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {Columns} FROM Users WHERE Status = $s ORDER BY Id DESC";
        cmd.Parameters.AddWithValue("$s", status.ToString());
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public void UpdateStatus(int id, AccountStatus status)
    {
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Users SET Status = $s WHERE Id = $id";
        cmd.Parameters.AddWithValue("$s", status.ToString());
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    private static User Map(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        FullName = r.GetString(1),
        Username = r.GetString(2),
        PasswordHash = r.GetString(3),
        Role = Enum.Parse<UserRole>(r.GetString(4)),
        Status = Enum.Parse<AccountStatus>(r.GetString(5)),
        CreatedDate = DateTime.Parse(r.GetString(6))
    };
}