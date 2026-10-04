using LostFound.Models;
using Microsoft.Data.Sqlite;

namespace LostFound.DAL;

public class ItemRepository : IItemRepository
{
    private const string Columns =
        "Id, Name, Category, Description, Location, Date, Status, ReporterId, ReporterName, ReporterContact, PickupLocation, MatchedItemId";

    public int Add(Item item)
    {
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Items
            (Name, Category, Description, Location, Date, Status, ReporterId, ReporterName, ReporterContact, PickupLocation, MatchedItemId)
            VALUES ($n, $c, $d, $l, $dt, $s, $rid, $rn, $rc, $pl, $mi)";
        cmd.Parameters.AddWithValue("$n", item.Name);
        cmd.Parameters.AddWithValue("$c", item.Category);
        cmd.Parameters.AddWithValue("$d", item.Description);
        cmd.Parameters.AddWithValue("$l", item.Location);
        cmd.Parameters.AddWithValue("$dt", item.Date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$s", item.Status.ToString());
        cmd.Parameters.AddWithValue("$rid", item.ReporterId);
        cmd.Parameters.AddWithValue("$rn", item.ReporterName);
        cmd.Parameters.AddWithValue("$rc", item.ReporterContact);
        cmd.Parameters.AddWithValue("$pl", item.PickupLocation);
        cmd.Parameters.AddWithValue("$mi", item.MatchedItemId);
        cmd.ExecuteNonQuery();

        var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid()";
        return Convert.ToInt32(idCmd.ExecuteScalar());
    }

    public List<Item> GetAll()
    {
        var list = new List<Item>();
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {Columns} FROM Items ORDER BY Id DESC";
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public Item? GetById(int id)
    {
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {Columns} FROM Items WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Map(r) : null;
    }

    public void UpdateStatus(int id, ItemStatus status)
    {
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Items SET Status = $s WHERE Id = $id";
        cmd.Parameters.AddWithValue("$s", status.ToString());
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public void SetMatch(int id, ItemStatus status, int matchedItemId, string pickupLocation)
    {
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE Items
                            SET Status = $s, MatchedItemId = $m, PickupLocation = $p
                            WHERE Id = $id";
        cmd.Parameters.AddWithValue("$s", status.ToString());
        cmd.Parameters.AddWithValue("$m", matchedItemId);
        cmd.Parameters.AddWithValue("$p", pickupLocation);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = new SqliteConnection(Database.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Items WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    private static Item Map(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1),
        Category = r.IsDBNull(2) ? "" : r.GetString(2),
        Description = r.IsDBNull(3) ? "" : r.GetString(3),
        Location = r.IsDBNull(4) ? "" : r.GetString(4),
        Date = DateTime.Parse(r.GetString(5)),
        Status = Enum.Parse<ItemStatus>(r.GetString(6)),
        ReporterId = r.GetInt32(7),
        ReporterName = r.IsDBNull(8) ? "" : r.GetString(8),
        ReporterContact = r.IsDBNull(9) ? "" : r.GetString(9),
        PickupLocation = r.IsDBNull(10) ? "" : r.GetString(10),
        MatchedItemId = r.GetInt32(11)
    };
}