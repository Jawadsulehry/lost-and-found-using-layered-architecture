using LostFound.BLL;
using LostFound.Models;

namespace LostFound.UI;

public class AdminDashboard : Form
{
    private readonly AdminService _admins;
    private readonly ItemService _items;
    private readonly HandoverService _handover;

    private readonly TabControl tabs = new();
    private readonly TabPage tabRequests = new("Account requests");
    private readonly TabPage tabNotes = new("Notifications");
    private readonly TabPage tabItems = new("All items");
    private readonly DataGridView gridRequests = NewGrid(), gridNotes = NewGrid(), gridItems = NewGrid();
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 5000 };

    public AdminDashboard(User admin, AdminService admins, ItemService items, HandoverService handover)
    {
        _admins = admins;
        _items = items;
        _handover = handover;

        AutoScaleMode = AutoScaleMode.Dpi;
        Text = "Lost & Found - Admin dashboard";
        ClientSize = new Size(1050, 620);
        StartPosition = FormStartPosition.CenterScreen;

        // Fill controls are added first, then the Top/Bottom panels
        tabRequests.Controls.Add(gridRequests);
        tabRequests.Controls.Add(ButtonBar(
            ("Approve account", () => Decide(true)),
            ("Reject account", () => Decide(false))));

        tabNotes.Controls.Add(gridNotes);
        tabNotes.Controls.Add(ButtonBar(
            ("Mark as read", () => MarkSelectedRead()),
            ("Mark all as read", () => { _admins.MarkAllRead(); RefreshAll(); }
        )));

        tabItems.Controls.Add(gridItems);
        tabItems.Controls.Add(ButtonBar(
            ("Request handover", () => ItemAction(id => _handover.RequestHandover(id),
                "The finder has been asked to hand the item in at the Admin Office.")),
            ("Received at office", () => ItemAction(id => _handover.ReceiveAtOffice(id))),
            ("Link to lost report", () => LinkSelected()),
            ("Mark as Returned", () => ItemAction(id => _handover.MarkReturned(id),
                "Item marked as returned."))));

        tabs.Dock = DockStyle.Fill;
        tabs.TabPages.AddRange(new[] { tabRequests, tabNotes, tabItems });
        Controls.Add(tabs);

        var header = new Panel { Dock = DockStyle.Top, Height = 52 };
        header.Controls.Add(new Label
        {
            Text = $"Admin dashboard  -  logged in as {admin.FullName}",
            Left = 15,
            Top = 16,
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold)
        });
        var btnLogout = new Button { Text = "Log out", Dock = DockStyle.Right, Width = 130 };
        btnLogout.Click += (s, e) => Close();
        header.Controls.Add(btnLogout);
        Controls.Add(header);

        timer.Tick += (s, e) => RefreshAll();
        FormClosed += (s, e) => { timer.Stop(); timer.Dispose(); };
        timer.Start();

        RefreshAll();
    }

    private static DataGridView NewGrid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = SystemColors.Window
    };

    private static Panel ButtonBar(params (string Text, Action Click)[] buttons)
    {
        var p = new Panel { Dock = DockStyle.Bottom, Height = 58 };
        int x = 10;
        foreach (var (text, click) in buttons)
        {
            var b = new Button { Text = text, Left = x, Top = 10, Width = 195, Height = 38 };
            b.Click += (s, e) => click();
            p.Controls.Add(b);
            x += 205;
        }
        return p;
    }

    private int? SelectedId(DataGridView g)
    {
        if (g.CurrentRow == null || !g.Columns.Contains("Id"))
        {
            MessageBox.Show("Select a row in the table first.", "Nothing selected");
            return null;
        }
        return (int)g.CurrentRow.Cells["Id"].Value;
    }

    private void Decide(bool approve)
    {
        var id = SelectedId(gridRequests);
        if (id == null) return;
        try
        {
            if (approve) _admins.ApproveAccount(id.Value);
            else _admins.RejectAccount(id.Value);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Not allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        RefreshAll();
    }

    private void MarkSelectedRead()
    {
        var id = SelectedId(gridNotes);
        if (id == null) return;
        _admins.MarkRead(id.Value);
        RefreshAll();
    }

    // Runs one workflow step on the selected item
    private void ItemAction(Action<int> action, string? successMessage = null)
    {
        var id = SelectedId(gridItems);
        if (id == null) return;
        try
        {
            action(id.Value);
            if (successMessage != null)
                MessageBox.Show(successMessage, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Not allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        RefreshAll();
    }

    private void LinkSelected()
    {
        var id = SelectedId(gridItems);
        if (id == null) return;
        try
        {
            var found = _items.GetById(id.Value)
                ?? throw new InvalidOperationException("Item not found.");
            if (found.Status != ItemStatus.InOffice)
                throw new InvalidOperationException(
                    "Select a Found item that has been received at the office first.");

            var candidates = _handover.GetCandidates(found);
            if (candidates.Count == 0)
            {
                MessageBox.Show("There are no open lost reports to match with.", "No lost reports");
                return;
            }

            using var dlg = new LinkDialog(found.Name, candidates);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            _handover.LinkAndNotify(found.Id, dlg.SelectedLostId);
            MessageBox.Show("Linked. The owner has been notified to collect the item from the Admin Office.",
                "Owner notified", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Not allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        RefreshAll();
    }

    // Keeps the selected row when the tables reload
    private static void Bind(DataGridView g, object data)
    {
        int? keep = null;
        if (g.CurrentRow != null && g.Columns.Contains("Id"))
            keep = g.CurrentRow.Cells["Id"].Value as int?;

        g.DataSource = data;

        if (keep != null)
            foreach (DataGridViewRow row in g.Rows)
                if (row.Cells["Id"].Value is int id && id == keep)
                {
                    g.CurrentCell = row.Cells[0];
                    break;
                }
    }

    private void RefreshAll()
    {
        var requests = _admins.GetPendingRequests()
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Username,
                Requested = u.CreatedDate.ToString("yyyy-MM-dd HH:mm")
            }).ToList();
        Bind(gridRequests, requests);

        var inbox = _admins.GetInbox();
        Bind(gridNotes, inbox.Select(n => new
        {
            n.Id,
            n.Type,
            n.Message,
            Received = n.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
            Read = n.IsRead ? "Yes" : "No"
        }).ToList());

        Bind(gridItems, _items.GetAll().Select(i => new
        {
            i.Id,
            i.Name,
            i.Category,
            i.Location,
            Date = i.Date.ToString("yyyy-MM-dd"),
            Status = i.Status.ToString(),
            Reporter = i.ReporterName,
            Contact = i.ReporterContact,
            Pickup = i.PickupLocation,
            LinkedTo = i.MatchedItemId == 0 ? "" : "#" + i.MatchedItemId
        }).ToList());

        tabRequests.Text = $"Account requests ({requests.Count})";
        int unread = inbox.Count(n => !n.IsRead);
        tabNotes.Text = unread > 0 ? $"Notifications ({unread} new)" : "Notifications";
    }
}