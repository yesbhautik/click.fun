using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using ClickTracker.Models;
using ClickTracker.Services;

namespace ClickTracker.Forms
{
    public partial class ServerSettingsForm : Form
    {
        private readonly StorageService _storageService;
        private readonly ComboBox _serverComboBox;
        private readonly TextBox _connectionStringTextBox;
        private AppSettings _settings;

        public ServerSettingsForm()
        {
            InitializeComponent();
            _storageService = new StorageService();
            _settings = _storageService.LoadSettings();

            this.Text = "Server Settings";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Server selection
            var serverLabel = new Label
            {
                Text = "Select Server:",
                Location = new Point(20, 20),
                AutoSize = true
            };

            _serverComboBox = new ComboBox
            {
                Location = new Point(20, 40),
                Width = 440,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Connection string
            var connectionLabel = new Label
            {
                Text = "Connection String:",
                Location = new Point(20, 80),
                AutoSize = true
            };

            _connectionStringTextBox = new TextBox
            {
                Location = new Point(20, 100),
                Width = 440
            };

            // Buttons
            var addButton = new Button
            {
                Text = "Add Server",
                Location = new Point(20, 140),
                Width = 100
            };

            var removeButton = new Button
            {
                Text = "Remove",
                Location = new Point(130, 140),
                Width = 100
            };

            var testButton = new Button
            {
                Text = "Test Connection",
                Location = new Point(240, 140),
                Width = 100
            };

            var saveButton = new Button
            {
                Text = "Save",
                Location = new Point(290, 200),
                Width = 80
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(380, 200),
                Width = 80
            };

            // Add controls
            this.Controls.AddRange(new Control[] {
                serverLabel,
                _serverComboBox,
                connectionLabel,
                _connectionStringTextBox,
                addButton,
                removeButton,
                testButton,
                saveButton,
                cancelButton
            });

            // Wire up events
            _serverComboBox.SelectedIndexChanged += ServerComboBox_SelectedIndexChanged;
            addButton.Click += AddButton_Click;
            removeButton.Click += RemoveButton_Click;
            testButton.Click += TestButton_Click;
            saveButton.Click += SaveButton_Click;
            cancelButton.Click += (s, e) => this.Close();

            // Load servers
            LoadServers();
        }

        private void LoadServers()
        {
            _serverComboBox.Items.Clear();
            foreach (var server in _settings.Servers)
            {
                _serverComboBox.Items.Add(server.Name);
            }

            if (_serverComboBox.Items.Count > 0)
            {
                _serverComboBox.SelectedItem = _settings.SelectedServer;
            }
        }

        private void ServerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_serverComboBox.SelectedItem != null)
            {
                var server = _settings.Servers.Find(s => s.Name == _serverComboBox.SelectedItem.ToString());
                if (server != null)
                {
                    _connectionStringTextBox.Text = server.ConnectionString;
                }
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            using (var form = new TextInputDialog("Add Server", "Server Name:"))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    string serverName = form.InputText;
                    if (string.IsNullOrWhiteSpace(serverName))
                    {
                        MessageBox.Show("Server name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (_settings.Servers.Any(s => s.Name.Equals(serverName, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("A server with this name already exists.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _settings.Servers.Add(new ServerConfig
                    {
                        Name = serverName,
                        ConnectionString = "http://localhost:3000",
                        AddedDate = DateTime.UtcNow
                    });

                    LoadServers();
                    _serverComboBox.SelectedItem = serverName;
                }
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (_serverComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a server to remove.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var server = _settings.Servers.Find(s => s.Name == _serverComboBox.SelectedItem.ToString());
            if (server.IsDefault)
            {
                MessageBox.Show("Cannot remove the default server.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to remove this server?", "Confirm Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _settings.Servers.Remove(server);
                if (_settings.SelectedServer == server.Name)
                {
                    _settings.SelectedServer = _settings.Servers.First().Name;
                }
                LoadServers();
            }
        }

        private async void TestButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_connectionStringTextBox.Text))
            {
                MessageBox.Show("Please enter a connection string.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var client = new System.Net.Http.HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(_connectionStringTextBox.Text);
                    var response = await client.GetAsync("/api/health");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Connection successful!", "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Connection failed. Server returned: " + response.StatusCode, "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection failed: " + ex.Message, "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (_serverComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a server.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_connectionStringTextBox.Text))
            {
                MessageBox.Show("Please enter a connection string.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var server = _settings.Servers.Find(s => s.Name == _serverComboBox.SelectedItem.ToString());
            if (server != null)
            {
                server.ConnectionString = _connectionStringTextBox.Text;
                _settings.SelectedServer = server.Name;
                _storageService.SaveSettings(_settings);
                MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }

    public class TextInputDialog : Form
    {
        private TextBox textBox;
        public string InputText => textBox.Text;

        public TextInputDialog(string title, string prompt)
        {
            this.Text = title;
            this.Size = new Size(300, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var label = new Label
            {
                Text = prompt,
                Location = new Point(10, 20),
                AutoSize = true
            };

            textBox = new TextBox
            {
                Location = new Point(10, 40),
                Width = 260
            };

            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(110, 70),
                Width = 75
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(195, 70),
                Width = 75
            };

            this.Controls.AddRange(new Control[] { label, textBox, okButton, cancelButton });
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
    }
} 