using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ClickTracker.Models;

namespace ClickTracker.Services
{
    public class AuthService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly StorageService _storageService;
        private bool _disposed;

        public AuthService()
        {
            _httpClient = new HttpClient();
            _storageService = new StorageService();
            var settings = _storageService.LoadSettings();
            var server = settings.Servers.Find(s => s.Name == settings.SelectedServer);
            if (server != null)
            {
                _httpClient.BaseAddress = new Uri(server.ConnectionString);
            }
        }

        public async Task<AuthResponse> LoginAsync(string usernameOrEmail, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    UsernameOrEmail = usernameOrEmail,
                    Password = password
                };

                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/auth/login", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);

                if (authResponse.Success)
                {
                    var settings = _storageService.LoadSettings();
                    settings.LastUsername = authResponse.User.Username;
                    settings.AuthToken = authResponse.Token;
                    _storageService.SaveSettings(settings);
                }

                return authResponse;
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Login failed: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> RegisterAsync(string username, string email, string password, string confirmPassword)
        {
            try
            {
                var request = new RegisterRequest
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    ConfirmPassword = confirmPassword
                };

                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/auth/register", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Registration failed: {ex.Message}"
                };
            }
        }

        public async Task<bool> ValidateTokenAsync()
        {
            try
            {
                var settings = _storageService.LoadSettings();
                if (string.IsNullOrEmpty(settings.AuthToken))
                    return false;

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", settings.AuthToken);
                var response = await _httpClient.GetAsync("/api/auth/validate");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public void Logout()
        {
            var settings = _storageService.LoadSettings();
            settings.AuthToken = null;
            _storageService.SaveSettings(settings);
        }

        public void UpdateServer(string serverName)
        {
            var settings = _storageService.LoadSettings();
            var server = settings.Servers.Find(s => s.Name == serverName);
            if (server != null)
            {
                _httpClient.BaseAddress = new Uri(server.ConnectionString);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }
    }
} 