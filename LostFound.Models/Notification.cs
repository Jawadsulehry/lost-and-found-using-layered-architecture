namespace LostFound.Models;

public class Notification
{
    public int Id { get; set; }
    public int RecipientUserId { get; set; }   // 0 = goes to the Admin inbox
    public string Type { get; set; } = "";     // AccountRequest, NewLostReport, NewFoundReport, HandoverRequest, CollectItem
    public string Message { get; set; } = "";
    public int RelatedId { get; set; }         // user id or item id it refers to
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}