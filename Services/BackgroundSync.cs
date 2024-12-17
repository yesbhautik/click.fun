using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using ClickTracker.Models;

namespace ClickTracker.Services
{
    public class BackgroundSync : IDisposable
    {
        private readonly Timer _timer;
        private readonly ApiService _apiService;
        private readonly StorageService _storageService;
        private const int SYNC_INTERVAL = 60000; // 1 minute
        private bool _disposed;
        private readonly object _syncLock = new object();
        private int _syncFlag;
        private DateTime _lastLogTime = DateTime.MinValue;

        public BackgroundSync()
        {
            _apiService = new ApiService();
            _storageService = new StorageService();
            _timer = new Timer(SyncData, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            try
            {
                _timer.Change(0, SYNC_INTERVAL);
                Debug.WriteLine("BackgroundSync: Service started");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BackgroundSync: Failed to start service - {ex.Message}");
            }
        }

        private async void SyncData(object state)
        {
            if (_disposed) return;

            // Try to acquire sync lock using interlocked operation
            if (Interlocked.Exchange(ref _syncFlag, 1) == 1)
            {
                return; // Already syncing
            }

            try
            {
                var settings = _storageService.LoadSettings();
                if (string.IsNullOrEmpty(settings.LastUsername))
                {
                    Debug.WriteLine("BackgroundSync: No username found, skipping sync");
                    return;
                }

                // Get current counts without resetting them
                int currentMouseClicks = InputTracker.MouseClickCount;
                int currentKeyboardPresses = InputTracker.KeyboardPressCount;

                // Only proceed if there are counts to sync
                if (currentMouseClicks > 0 || currentKeyboardPresses > 0)
                {
                    var clickData = new ClickData
                    {
                        MouseClicks = currentMouseClicks,
                        KeyboardPresses = currentKeyboardPresses,
                        Timestamp = DateTime.UtcNow,
                        Username = settings.LastUsername
                    };

                    // Log only once every minute
                    var now = DateTime.UtcNow;
                    if ((now - _lastLogTime).TotalMinutes >= 1)
                    {
                        Debug.WriteLine($"BackgroundSync: Syncing for user {settings.LastUsername} - Mouse: {currentMouseClicks}, Keyboard: {currentKeyboardPresses}");
                        _lastLogTime = now;
                    }

                    // Try to sync
                    bool syncSuccess = await _apiService.SyncClickData(clickData).ConfigureAwait(false);
                    if (syncSuccess)
                    {
                        lock (_syncLock)
                        {
                            // Only reset counters if the current counts haven't changed
                            if (currentMouseClicks == InputTracker.MouseClickCount &&
                                currentKeyboardPresses == InputTracker.KeyboardPressCount)
                            {
                                InputTracker.ResetCounters();
                                _storageService.SaveClickData(clickData);
                                Debug.WriteLine("BackgroundSync: Successfully synced and reset counters");
                            }
                            else
                            {
                                Debug.WriteLine("BackgroundSync: Counts changed during sync, will sync delta in next interval");
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("BackgroundSync: Sync failed, will retry in next interval");
                    }
                }
            }
            catch (HttpRequestException)
            {
                // Ignore network-related errors as they're expected when offline
                Debug.WriteLine("BackgroundSync: Network error, will retry next sync interval");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BackgroundSync: Critical error - {ex.Message}");
            }
            finally
            {
                // Release the sync lock
                Interlocked.Exchange(ref _syncFlag, 0);
            }
        }

        public void Stop()
        {
            try
            {
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                Debug.WriteLine("BackgroundSync: Service stopped");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BackgroundSync: Failed to stop service - {ex.Message}");
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
                    Stop();
                    _timer?.Dispose();
                    _apiService?.Dispose();
                }
                _disposed = true;
            }
        }
    }
} 