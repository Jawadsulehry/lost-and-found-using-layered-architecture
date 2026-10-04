using LostFound.BLL;
using LostFound.Models;

namespace LostFound.UI;

public class MemberDashboard : Form
{
    private readonly User _user;
    private readonly ItemService _items;
    private readonly NotificationService _notes;

    private readonly TabControl tabs = new();
    private readonly TabPage tabReport = new("Report an item");
    private readonly TabPage tabMine = new("My reports");
    private readonly TabPage tabNotes = new("Notifications");
    private readonly TabPage tabBrowse = new("Search items");

    private readonly TextBox txtName = new(), txtLocation = new(), txtContact = new(),
                             txtDesc = new(), txtSearch = new();
    private readonly ComboBox cmbCategory = new(), cmbType = new();
    private readonly DateTimePicker dtpDate = new();
    private readonly DataGridView gridMine = NewGrid(), gridNotes = NewGrid(), gridBrowse = NewGrid();
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 5000 };

    private static readonly string[] Categories =
        { "Wallet", "Phone", "Keys", "Bag", "Documents", "Electronics", "Clothing", "Other" };

    public MemberDashboard(User user, ItemService items, NotificationService notes)
    {
        _user = user;
        _items = items;
        _notes = notes;

        AutoScaleMode = AutoScaleMode.Dpi;
        Text = "Lost & Found - Member dashboard";
        ClientSize = new Size(1000, 620);
        StartPosition = FormStartPosition.CenterScreen;

        BuildReportTab();

        tabMine.Controls.Add(gridMine);

        tabNotes.Controls.Add(gridNotes);
        tabNotes.Controls.Add(ButtonBar(
            ("Mark as read", () => MarkSelectedRead()),
            ("Mark all as read", () => { _notes.MarkAllRead(_user.Id); RefreshAll(); }
        )));

        tabBrowse.Controls.Add(gridBrowse);
        var searchBar = new Panel { Dock = DockStyle.Top, Height = 58 };
        searchBar.Controls.Add(new Label { Text = "Keyword", Left = 10, Top = 8, AutoSize = true });
        txtSearch.SetBounds(10, 27, 300, 27);
        searchBar.Controls.Add(txtSearch);
        var btnSearch = new Button { Text = "Search", Left = 325, Top = 22, Width = 120, Height = 34 };
        btnSearch.Click += (s, e) => RefreshBrowse();
        searchBar.Controls.Add(btnSearch);
        var btnClear = new Button { Text = "Clear", Left = 455, Top = 22, Width = 120, Height = 34 };
        btnClear.Click += (s, e) => { txtSearch.Clear(); RefreshBrowse(); };
        searchBar.Controls.Add(btnClear);
        tabBrowse.Controls.Add(searchBar);

        tabs.Dock = DockStyle.Fill;
        tabs.TabPages.AddRange(new[] { tabReport, tabMine, tabNotes, tabBrowse });
        Controls.Add(tabs);

        var header = new Panel { Dock = DockStyle.Top, Height = 52 };
        header.Controls.Add(new Label
        {
            Text = $"Member dashboard  -  logged in as {_user.FullName} ({_user.Username})",
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

    private void BuildReportTab()
    {
        tabReport.AutoScroll = true;

        cmbType.Items.AddRange(new object[] { "Lost", "Found" });
        cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbType.SelectedIndex = 0;

        cmbCategory.Items.AddRange(Categories);
        cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCategory.SelectedIndex = 0;

        dtpDate.Format = DateTimePickerFormat.Short;
        dtpDate.MaxDate = DateTime.Today;

        txtDesc.Multiline = true;

        Field("I want to report an item that I", cmbType, 20, 15, 200);
        Field("Item name", txtName, 20, 75, 300);
        Field("Category", cmbCategory, 340, 75, 200);
        Field("Where was it lost or found?", txtLocation, 20, 135, 300);
        Field("Date", dtpDate, 340, 135, 200);
        Field("Your contact (phone or email)", txtContact, 20, 195, 300);
        Field("Description (colour, brand, marks)", txtDesc, 20, 255, 520);
        txtDesc.Height = 90;

        var btn = new Button { Text = "Send report", Left = 20, Top = 380, Width = 220, Height = 40 };
        btn.Click += (s, e) => SubmitReport();
        tabReport.Controls.Add(btn);

        tabReport.Controls.Add(new Label
        {
            Text = "The admin is notified of every report. For a found item, the admin may ask you to\r\n" +
                   "hand it in at the admin office. Watch the Notifications tab for messages.",
            Left = 20,
            Top = 440,
            AutoSize = true
        });
    }

    private void Field(string label, Control c, int x, int y, int w)
    {
        tabReport.Controls.Add(new Label { Text = label, Left = x, Top = y, AutoSize = true });
        c.Left = x;
        c.Top = y + 24;
        c.Width = w;
        tabReport.Controls.Add(c);
    }

    private void SubmitReport()
    {
        try
        {
            _items.Report(new Item
            {
                Name = txtName.Text,
                Category = cmbCategory.Text,
                Location = txtLocation.Text,
                Date = dtpDate.Value.Date,
                Description = txtDesc.Text.Trim(),
                ReporterId = _user.Id,
                ReporterName = _user.FullName,
                ReporterContact = txtContact.Text,
                Status = Enum.Parse<ItemStatus>(cmbType.Text)
            });

            txtName.Clear();
            txtLocation.Clear();
            txtDesc.Clear();
            MessageBox.Show("Report sent. The admin has been notified.", "Report sent",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshAll();
            tabs.SelectedTab = tabMine;
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "Check your input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
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
            var b = new Button { Text = text, Left = x, Top = 10, Width = 180, Height = 38 };
            b.Click += (s, e) => click();
            p.Controls.Add(b);
            x += 190;
        }
        return p;
    }

    private void MarkSelectedRead()
    {
        if (gridNotes.CurrentRow == null || !gridNotes.Columns.Contains("Id"))
        {
            MessageBox.Show("Select a notification first.", "Nothing selected");
            return;
        }
        _notes.MarkRead((int)gridNotes.CurrentRow.Cells["Id"].Value);
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

    private void RefreshBrowse()
    {
        // Other members' contact details are not shown here
        Bind(gridBrowse, _items.Search(txtSearch.Text, null, null, null, null)
            .Select(i => new
            {
                i.Id,
                i.Name,
                i.Category,
                i.Location,
                Date = i.Date.ToString("yyyy-MM-dd"),
                Status = i.Status.ToString()
            }).ToList());
    }

    private void RefreshAll()
    {
        var mine = _items.GetByReporter(_user.Id);
        Bind(gridMine, mine.Select(i => new
        {
            i.Id,
            i.Name,
            i.Category,
            i.Location,
            Date = i.Date.ToString("yyyy-MM-dd"),
            Status = i.Status.ToString(),
            Pickup = i.PickupLocation
        }).ToList());

        var inbox = _notes.GetFor(_user.Id);
        Bind(gridNotes, inbox.Select(n => new
        {
            n.Id,
            n.Type,
            n.Message,
            Received = n.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
            Read = n.IsRead ? "Yes" : "No"
        }).ToList());

        RefreshBrowse();

        tabMine.Text = $"My reports ({mine.Count})";
        int unread = inbox.Count(n => !n.IsRead);
        tabNotes.Text = unread > 0 ? $"Notifications ({unread} new)" : "Notifications";
    }
}