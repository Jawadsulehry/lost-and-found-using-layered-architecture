using Microsoft.Data.Sqlite;

namespace LostFound.DAL;

public static class Database
{
    public static readonly string ConnectionString = "Data Source=lostfound.db";

    public static void Initialize()
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                Role TEXT NOT NULL,
                Status TEXT NOT NULL,
                CreatedDate TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Items (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Category TEXT,
                Description TEXT,
                Location TEXT,
                Date TEXT NOT NULL,
                Status TEXT NOT NULL,
                ReporterId INTEGER NOT NULL DEFAULT 0,
                ReporterName TEXT,
                ReporterContact TEXT,
                PickupLocation TEXT,
                MatchedItemId INTEGER NOT NULL DEFAULT 0
            );
            CREATE TABLE IF NOT EXISTS Notifications (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RecipientUserId INTEGER NOT NULL,
                Type TEXT NOT NULL,
                Message TEXT NOT NULL,
                RelatedId INTEGER NOT NULL DEFAULT 0,
                IsRead INTEGER NOT NULL DEFAULT 0,
                CreatedDate TEXT NOT NULL
            );";
        cmd.ExecuteNonQuery();
    }
}