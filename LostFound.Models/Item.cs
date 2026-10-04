namespace LostFound.Models;

public enum ItemStatus { Lost, Found, AwaitingHandover, InOffice, Matched, Returned }

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string Location { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Today;
    public ItemStatus Status { get; set; }
    public int ReporterId { get; set; }
    public string ReporterName { get; set; } = "";
    public string ReporterContact { get; set; } = "";
    public string PickupLocation { get; set; } = "";
    public int MatchedItemId { get; set; }   // links a Found item to a Lost report (0 = none)
}