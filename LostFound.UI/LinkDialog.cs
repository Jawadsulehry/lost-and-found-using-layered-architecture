using LostFound.Models;

namespace LostFound.UI;

// Lets the admin pick which Lost report matches a Found item
public class LinkDialog : Form
{
    private readonly DataGridView grid = new();
    public int SelectedLostId { get; private set; }

    public LinkDialog(string foundName, List<(Item Item, int Score)> candidates)
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = "Link to a lost report";
        ClientSize = new Size(820, 440);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = SystemColors.Window;
        grid.DataSource = candidates.Select(c => new
        {
            c.Item.Id,
            c.Item.Name,
            c.Item.Category,
            c.Item.Location,
            Date = c.Item.Date.ToString("yyyy-MM-dd"),
            Owner = c.Item.ReporterName,
            MatchScore = c.Score
        }).ToList();
        Controls.Add(grid);

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 58 };
        var ok = new Button { Text = "Link and notify owner", Left = 10, Top = 10, Width = 220, Height = 38 };
        ok.Click += (s, e) =>
        {
            if (grid.CurrentRow == null) return;
            SelectedLostId = (int)grid.CurrentRow.Cells["Id"].Value;
            DialogResult = DialogResult.OK;
        };
        var cancel = new Button { Text = "Cancel", Left = 240, Top = 10, Width = 140, Height = 38 };
        cancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
        bottom.Controls.Add(ok);
        bottom.Controls.Add(cancel);
        Controls.Add(bottom);

        Controls.Add(new Label
        {
            Text = $"Choose the lost report that matches \"{foundName}\". Best matches are at the top.",
            Dock = DockStyle.Top,
            Height = 36,
            Padding = new Padding(10, 10, 0, 0)
        });
    }
}