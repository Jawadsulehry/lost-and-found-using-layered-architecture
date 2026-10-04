using LostFound.BLL;

namespace LostFound.UI;

public class RegisterForm : Form
{
    private readonly AuthService _auth;
    private readonly TextBox txtName = new(), txtUser = new(), txtPass = new(), txtConfirm = new();

    public RegisterForm(AuthService auth)
    {
        _auth = auth;
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = "Create account";
        ClientSize = new Size(380, 360);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        AddField("Full name", txtName, 15);
        AddField("Username", txtUser, 75);
        AddField("Password (min 6 characters)", txtPass, 135);
        AddField("Confirm password", txtConfirm, 195);
        txtPass.UseSystemPasswordChar = true;
        txtConfirm.UseSystemPasswordChar = true;

        var btn = new Button { Text = "Send account request", Left = 20, Top = 265, Width = 340, Height = 38 };
        btn.Click += (s, e) => Submit();
        Controls.Add(btn);
        AcceptButton = btn;
    }

    private void AddField(string label, TextBox box, int top)
    {
        Controls.Add(new Label { Text = label, Left = 20, Top = top, AutoSize = true });
        box.SetBounds(20, top + 22, 340, 27);
        Controls.Add(box);
    }

    private void Submit()
    {
        try
        {
            _auth.Register(txtName.Text, txtUser.Text, txtPass.Text, txtConfirm.Text);
            MessageBox.Show("Request sent. You can log in after the admin approves your account.",
                "Request sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "Check your input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}