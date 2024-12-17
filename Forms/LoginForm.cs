using System;
using System.Drawing;
using System.Windows.Forms;
using ClickTracker.Services;

namespace ClickTracker.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;
        private readonly TabControl tabControl;
        private readonly TextBox loginUsername;
        private readonly TextBox loginPassword;
        private readonly TextBox regUsername;
        private readonly TextBox regEmail;
        private readonly TextBox regPassword;
        private readonly TextBox regConfirmPassword;
        private bool _loginSuccess;

        public LoginForm()
        {
            InitializeComponent();
            _authService = new AuthService();

            this.Text = "ClickTracker - Login";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Create tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(10, 10)
            };

            // Login Tab
            var loginTab = new TabPage("Login");
            loginTab.Padding = new Padding(20);
            
            loginUsername = new TextBox { Width = 250, Location = new Point(50, 50) };
            loginPassword = new TextBox { Width = 250, Location = new Point(50, 100), UseSystemPasswordChar = true };
            var loginButton = new Button { Text = "Login", Width = 250, Location = new Point(50, 150) };

            loginTab.Controls.AddRange(new Control[] {
                new Label { Text = "Username/Email:", Location = new Point(50, 30) },
                loginUsername,
                new Label { Text = "Password:", Location = new Point(50, 80) },
                loginPassword,
                loginButton
            });

            // Register Tab
            var registerTab = new TabPage("Register");
            registerTab.Padding = new Padding(20);

            regUsername = new TextBox { Width = 250, Location = new Point(50, 50) };
            regEmail = new TextBox { Width = 250, Location = new Point(50, 100) };
            regPassword = new TextBox { Width = 250, Location = new Point(50, 150), UseSystemPasswordChar = true };
            regConfirmPassword = new TextBox { Width = 250, Location = new Point(50, 200), UseSystemPasswordChar = true };
            var registerButton = new Button { Text = "Register", Width = 250, Location = new Point(50, 250) };

            registerTab.Controls.AddRange(new Control[] {
                new Label { Text = "Username:", Location = new Point(50, 30) },
                regUsername,
                new Label { Text = "Email:", Location = new Point(50, 80) },
                regEmail,
                new Label { Text = "Password:", Location = new Point(50, 130) },
                regPassword,
                new Label { Text = "Confirm Password:", Location = new Point(50, 180) },
                regConfirmPassword,
                registerButton
            });

            // Add tabs to control
            tabControl.TabPages.AddRange(new TabPage[] { loginTab, registerTab });
            this.Controls.Add(tabControl);

            // Wire up events
            loginButton.Click += LoginButton_Click;
            registerButton.Click += RegisterButton_Click;

            // Add server settings button
            var settingsButton = new Button
            {
                Text = "Server Settings",
                Width = 120,
                Location = new Point(this.ClientSize.Width - 140, this.ClientSize.Height - 60)
            };
            settingsButton.Click += SettingsButton_Click;
            this.Controls.Add(settingsButton);
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(loginUsername.Text) || string.IsNullOrWhiteSpace(loginPassword.Text))
            {
                MessageBox.Show("Please enter both username/email and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var loadingForm = new LoadingForm())
            {
                loadingForm.Show(this);
                var response = await _authService.LoginAsync(loginUsername.Text, loginPassword.Text);
                loadingForm.Close();

                if (response.Success)
                {
                    _loginSuccess = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(response.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void RegisterButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(regUsername.Text) || 
                string.IsNullOrWhiteSpace(regEmail.Text) || 
                string.IsNullOrWhiteSpace(regPassword.Text) || 
                string.IsNullOrWhiteSpace(regConfirmPassword.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (regPassword.Text != regConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var loadingForm = new LoadingForm())
            {
                loadingForm.Show(this);
                var response = await _authService.RegisterAsync(regUsername.Text, regEmail.Text, regPassword.Text, regConfirmPassword.Text);
                loadingForm.Close();

                if (response.Success)
                {
                    MessageBox.Show("Registration successful! Please login.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    tabControl.SelectedIndex = 0; // Switch to login tab
                    loginUsername.Text = regUsername.Text;
                }
                else
                {
                    MessageBox.Show(response.Message, "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new ServerSettingsForm())
            {
                settingsForm.ShowDialog(this);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (!_loginSuccess && e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }
    }
} 