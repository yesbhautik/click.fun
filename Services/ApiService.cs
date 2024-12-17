using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using ClickTracker.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace ClickTracker.Services
{
    public class ApiService : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _apiUrl;
        private readonly StorageService _storageService;
        private bool _disposed;

        public ApiService()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(30);
            _storageService = new StorageService();

            // Get API endpoint from configuration
            _apiUrl = ConfigurationManager.AppSettings["ApiEndpoint"] ?? "http://localhost:3000/api";
            if (string.IsNullOrEmpty(_apiUrl))
            {
                throw new ConfigurationErrorsException("API endpoint is not configured. Please set the ApiEndpoint in App.config.");
            }

            // Set up authorization header
            var settings = _storageService.LoadSettings();
            if (!string.IsNullOrEmpty(settings.AuthToken))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.AuthToken);
                Debug.WriteLine($"ApiService: Set auth token: {settings.AuthToken.Substring(0, 10)}...");
            }
            else
            {
                Debug.WriteLine("ApiService: No auth token found");
            }

            Debug.WriteLine($"ApiService: Initialized with endpoint {_apiUrl}");
        }

        public async Task<bool> SyncClickData(ClickData data)
        {
            try
            {
                if (data == null)
                {
                    Debug.WriteLine("SyncClickData: Data is null");
                    return false;
                }

                var settings = _storageService.LoadSettings();
                if (string.IsNullOrEmpty(settings.LastUsername))
                {
                    Debug.WriteLine("SyncClickData: No username found");
                    return false;
                }

                // Update authorization header in case token has changed
                if (!string.IsNullOrEmpty(settings.AuthToken))
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.AuthToken);
                    Debug.WriteLine($"SyncClickData: Using auth token: {settings.AuthToken.Substring(0, 10)}...");
                }
                else
                {
                    Debug.WriteLine("SyncClickData: No auth token available");
                    return false;
                }

                // Create the request object that matches the API's expected format
                var requestData = new
                {
                    mouseClicks = data.MouseClicks,
                    keyboardPresses = data.KeyboardPresses,
                    timestamp = data.Timestamp,
                    username = settings.LastUsername
                };

                var json = JsonConvert.SerializeObject(requestData);
                Debug.WriteLine($"SyncClickData: Sending data: {json}");
                Debug.WriteLine($"SyncClickData: Headers: {string.Join(", ", _client.DefaultRequestHeaders)}");

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var endpoint = $"{_apiUrl}/clicks";
                Debug.WriteLine($"SyncClickData: Endpoint: {endpoint}");

                using (var response = await _client.PostAsync(endpoint, content))
                {
                    var statusCode = (int)response.StatusCode;
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"SyncClickData: Response status: {statusCode}, content: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
                        Debug.WriteLine($"SyncClickData: Successfully synced data. Response: {responseContent}");
                        return apiResponse?.Success ?? false;
                    }

                    Debug.WriteLine($"SyncClickData: API request failed with status code {statusCode}");
                    if (statusCode >= 500)
                    {
                        Debug.WriteLine("SyncClickData: Server error, data will be retried later");
                    }
                    else if (statusCode == 401 || statusCode == 403)
                    {
                        Debug.WriteLine("SyncClickData: Authentication/Authorization error");
                    }
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"SyncClickData: Network error occurred: {ex.Message}");
                return false;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"SyncClickData: JSON serialization error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SyncClickData: Unexpected error: {ex.Message}");
                return false;
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
                    _client?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}