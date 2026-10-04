using LostFound.BLL;
using LostFound.Models;

namespace LostFound.UI;

public class LoginForm : Form
{
    private readonly AuthService _auth;
    private readonly TextBox txtUser = new(), txtPass = new();
    public User? LoggedInUser { get; private set; }

    public LoginForm(AuthService auth)
    {
        _auth = auth;
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = "Lost & Found - Login";
        ClientSize = new Size(380, 270);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        Controls.Add(new Label
        {
            Text = "Lost & Found Management System",
            Left = 20,
            Top = 15,
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold)
        });

        Controls.Add(new Label { Text = "Username", Left = 20, Top = 60, AutoSize = true });
        txtUser.SetBounds(20, 82, 340, 27);
        Controls.Add(txtUser);

        Controls.Add(new Label { Text = "Password", Left = 20, Top = 120, AutoSize = true });
        txtPass.SetBounds(20, 142, 340, 27);
        txtPass.UseSystemPasswordChar = true;
        Controls.Add(txtPass);

        var btnLogin = new Button { Text = "Log in", Left = 20, Top = 195, Width = 160, Height = 36 };
        btnLogin.Click += (s, e) => DoLogin();
        Controls.Add(btnLogin);
        AcceptButton = btnLogin;

        var btnRegister = new Button { Text = "Create account", Left = 200, Top = 195, Width = 160, Height = 36 };
        btnRegister.Click += (s, e) =>
        {
            using var reg = new RegisterForm(_auth);
            reg.ShowDialog(this);
        };
        Controls.Add(btnRegister);
    }

    private void DoLogin()
    {
        try
        {
            LoggedInUser = _auth.Login(txtUser.Text, txtPass.Text);
            DialogResult = DialogResult.OK;
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}