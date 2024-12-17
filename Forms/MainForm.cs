using System;
using System.Drawing;
using System.Windows.Forms;
using ClickTracker.Services;
using ClickTracker.Models;

namespace ClickTracker.Forms
{
    public partial class MainForm : Form
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly Timer _updateTimer;
        private readonly InputTracker _inputTracker;
        private readonly BackgroundSync _backgroundSync;
        private readonly AuthService _authService;
        private readonly StorageService _storageService;
        private readonly Label _mouseClicksLabel;
        private readonly Label _keyboardPressesLabel;
        private readonly Label _usernameLabel;
        private bool _disposed;

        public MainForm()
        {
            InitializeComponent();
            _authService = new AuthService();
            _storageService = new StorageService();

            // Check if user is logged in
            var settings = _storageService.LoadSettings();
            if (string.IsNullOrEmpty(settings.AuthToken))
            {
                ShowLoginForm();
                return;
            }

            this.Text = "Click Tracker";
            this.Size = new Size(300, 200);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Username Label
            _usernameLabel = new Label
            {
                Text = $"User: {settings.LastUsername}",
                Location = new Point(20, 20),
                AutoSize = true
            };

            // Click counts labels
            _mouseClicksLabel = new Label
            {
                Text = "Mouse Clicks: 0",
                Location = new Point(20, 50),
                AutoSize = true
            };

            _keyboardPressesLabel = new Label
            {
                Text = "Keyboard Presses: 0",
                Location = new Point(20, 80),
                AutoSize = true
            };

            // Settings Button
            var settingsButton = new Button
            {
                Text = "Settings",
                Location = new Point(20, 120),
                Width = 100
            };
            settingsButton.Click += SettingsButton_Click;

            // Logout Button
            var logoutButton = new Button
            {
                Text = "Logout",
                Location = new Point(130, 120),
                Width = 100
            };
            logoutButton.Click += LogoutButton_Click;

            // Add controls
            this.Controls.AddRange(new Control[] {
                _usernameLabel,
                _mouseClicksLabel,
                _keyboardPressesLabel,
                settingsButton,
                logoutButton
            });

            // Initialize services
            _inputTracker = new InputTracker();
            _backgroundSync = new BackgroundSync();

            // Create notify icon
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "Click Tracker",
                Visible = true
            };

            // Create context menu
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, ShowWindow);
            contextMenu.Items.Add("Exit", null, Exit);
            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += ShowWindow;

            // Create update timer
            _updateTimer = new Timer
            {
                Interval = 1000 // Update every second
            };
            _updateTimer.Tick += UpdateLabels;
            _updateTimer.Start();

            // Initialize tracking
            InputTracker.Initialize();
            _backgroundSync.Start();

            // Handle minimize
            this.Resize += (s, e) =>
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    Hide();
                }
            };
        }

        private void ShowLoginForm()
        {
            var loginForm = new LoginForm();
            this.Hide();
            loginForm.ShowDialog();

            // Check if login was successful
            var settings = _storageService.LoadSettings();
            if (string.IsNullOrEmpty(settings.AuthToken))
            {
                Application.Exit();
                return;
            }

            _usernameLabel.Text = $"User: {settings.LastUsername}";
            this.Show();
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            _authService.Logout();
            ShowLoginForm();
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new ServerSettingsForm())
            {
                settingsForm.ShowDialog(this);
            }
        }

        private void UpdateLabels(object sender, EventArgs e)
        {
            _mouseClicksLabel.Text = $"Mouse Clicks: {InputTracker.MouseClickCount}";
            _keyboardPressesLabel.Text = $"Keyboard Presses: {InputTracker.KeyboardPressCount}";
        }

        private void ShowWindow(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void Exit(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _updateTimer?.Dispose();
                    _notifyIcon?.Dispose();
                    _backgroundSync?.Dispose();
                    _authService?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}