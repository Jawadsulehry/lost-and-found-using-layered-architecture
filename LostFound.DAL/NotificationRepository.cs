using LostFound.Models;
using Microsoft.Data.Sqlite;

namespace LostFound.DAL;

public class NotificationRepository : INotificationRepository
{
    public void Add(Notification n)
    {
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Notifications (RecipientUserId, Type, Message, RelatedId, IsRead, CreatedDate)
                            VALUES ($u, $t, $m, $r, 0, $d)";
        cmd.Parameters.AddWithValue("$u", n.RecipientUserId);
        cmd.Parameters.AddWithValue("$t", n.Type);
        cmd.Parameters.AddWithValue("$m", n.Message);
        cmd.Parameters.AddWithValue("$r", n.RelatedId);
        cmd.Parameters.AddWithValue("$d", n.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.ExecuteNonQuery();
    }

    public List<Notification> GetForUser(int userId)
    {
        var list = new List<Notification>();
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT Id, RecipientUserId, Type, Message, RelatedId, IsRead, CreatedDate
                            FROM Notifications WHERE RecipientUserId = $u ORDER BY Id DESC";
        cmd.Parameters.AddWithValue("$u", userId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new Notification
            {
                Id = r.GetInt32(0),
                RecipientUserId = r.GetInt32(1),
                Type = r.GetString(2),
                Message = r.GetString(3),
                RelatedId = r.GetInt32(4),
                IsRead = r.GetInt32(5) == 1,
                CreatedDate = DateTime.Parse(r.GetString(6))
            });
        }
        return list;
    }

    public void MarkRead(int id)
    {
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Notifications SET IsRead = 1 WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public int CountUnread(int userId)
    {
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Notifications WHERE RecipientUserId = $u AND IsRead = 0";
        cmd.Parameters.AddWithValue("$u", userId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}