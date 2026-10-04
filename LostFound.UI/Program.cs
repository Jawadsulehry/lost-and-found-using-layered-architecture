using LostFound.BLL;
using LostFound.Models;

namespace LostFound.UI;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        var auth = new AuthService();
        var admins = new AdminService();
        var items = new ItemService();
        var notes = new NotificationService();
        var handover = new HandoverService();

        // After a dashboard closes (log out), the login screen comes back
        while (true)
        {
            using var login = new LoginForm(auth);
            if (login.ShowDialog() != DialogResult.OK) break;

            var user = login.LoggedInUser!;
            if (user.Role == UserRole.Admin)
            {
                using var dashboard = new AdminDashboard(user, admins, items, handover);
                dashboard.ShowDialog();
            }
            else
            {
                using var dashboard = new MemberDashboard(user, items, notes);
                dashboard.ShowDialog();
            }
        }
    }
}