using LostFound.DAL;
using LostFound.Models;

namespace LostFound.BLL;

// The admin's workflow for Found items:
// Found -> AwaitingHandover -> InOffice -> Matched (linked to a Lost report) -> Returned
public class HandoverService
{
    public const string OfficeName = "Admin Office";

    private readonly IItemRepository _items;
    private readonly INotificationRepository _notes;

    public HandoverService() : this(new ItemRepository(), new NotificationRepository()) { }

    public HandoverService(IItemRepository items, INotificationRepository notes)
    {
        _items = items;
        _notes = notes;
        Database.Initialize();
    }

    private Item Get(int id) =>
        _items.GetById(id) ?? throw new InvalidOperationException("Item not found.");

    // Step 1: ask the finder (by their account) to bring the item to the office
    public void RequestHandover(int foundId)
    {
        var item = Get(foundId);
        if (item.Status != ItemStatus.Found)
            throw new InvalidOperationException(
                "Handover can only be requested for a Found item that has not been handled yet.");

        _items.UpdateStatus(foundId, ItemStatus.AwaitingHandover);
        _notes.Add(new Notification
        {
            RecipientUserId = item.ReporterId,
            Type = "HandoverRequest",
            Message = $"Thank you for reporting \"{item.Name}\". Please hand it in at the {OfficeName}.",
            RelatedId = foundId
        });
    }

    // Step 2: the finder arrived and the admin has the item
    public void ReceiveAtOffice(int foundId)
    {
        var item = Get(foundId);
        if (item.Status != ItemStatus.AwaitingHandover)
            throw new InvalidOperationException(
                "Click Request handover first. The finder must be asked to bring the item before it can be received.");

        _items.SetMatch(foundId, ItemStatus.InOffice, item.MatchedItemId, OfficeName);
    }

    // Suggested Lost reports for a Found item: same category and shared words rank first
    public List<(Item Item, int Score)> GetCandidates(Item found) =>
        _items.GetAll()
            .Where(i => i.Status == ItemStatus.Lost)
            .Select(i => (Item: i, Score: Score(found, i)))
            .OrderByDescending(c => c.Score)
            .ThenByDescending(c => c.Item.Date)
            .ToList();

    private static int Score(Item found, Item lost)
    {
        int score = 0;
        if (found.Category == lost.Category) score += 2;

        static HashSet<string> Words(string s) =>
            s.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries)
             .Where(w => w.Length >= 3).ToHashSet();

        var foundWords = Words(found.Name + " " + found.Description);
        var lostWords = Words(lost.Name + " " + lost.Description);
        score += 3 * foundWords.Intersect(lostWords).Count();
        return score;
    }

    // Step 3: link the Found item to the owner's Lost report and notify the owner
    public void LinkAndNotify(int foundId, int lostId)
    {
        var found = Get(foundId);
        var lost = Get(lostId);

        if (found.Status != ItemStatus.InOffice)
            throw new InvalidOperationException("The found item must be received at the office first.");
        if (lost.Status != ItemStatus.Lost)
            throw new InvalidOperationException("That lost report is not open.");

        _items.SetMatch(foundId, ItemStatus.Matched, lostId, OfficeName);
        _items.SetMatch(lostId, ItemStatus.Matched, foundId, OfficeName);

        _notes.Add(new Notification
        {
            RecipientUserId = lost.ReporterId,
            Type = "CollectItem",
            Message = $"Good news! Your lost item \"{lost.Name}\" has been found. " +
                      $"Please collect it from the {OfficeName} and bring your ID.",
            RelatedId = lostId
        });
    }

    // Step 4: the owner collected it. Both linked reports are closed
    public void MarkReturned(int itemId)
    {
        var item = Get(itemId);
        if (item.Status != ItemStatus.Matched)
            throw new InvalidOperationException("Only a matched item can be marked as Returned.");

        _items.UpdateStatus(itemId, ItemStatus.Returned);

        if (item.MatchedItemId > 0)
        {
            var partner = _items.GetById(item.MatchedItemId);
            if (partner != null && partner.Status == ItemStatus.Matched)
                _items.UpdateStatus(partner.Id, ItemStatus.Returned);
        }
    }
}