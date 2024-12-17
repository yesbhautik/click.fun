using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace ClickTracker.Services
{
    public class InputTracker
    {
        private const int WH_MOUSE_LL = 14;
        private const int WH_KEYBOARD_LL = 13;
        private static IntPtr mouseHookID = IntPtr.Zero;
        private static IntPtr keyboardHookID = IntPtr.Zero;

        private static long _mouseClickCount;
        private static long _keyboardPressCount;
        private static DateTime _lastLogTime = DateTime.MinValue;

        public static int MouseClickCount => (int)Interlocked.Read(ref _mouseClickCount);
        public static int KeyboardPressCount => (int)Interlocked.Read(ref _keyboardPressCount);

        private static LowLevelMouseProc mouseProc;
        private static LowLevelKeyboardProc keyboardProc;

        public static void Initialize()
        {
            mouseProc = HookMouseCallback;
            keyboardProc = HookKeyboardCallback;
            mouseHookID = SetMouseHook(mouseProc);
            keyboardHookID = SetKeyboardHook(keyboardProc);
            Debug.WriteLine("InputTracker: Initialized");
        }

        public static void ResetCounters()
        {
            Interlocked.Exchange(ref _mouseClickCount, 0);
            Interlocked.Exchange(ref _keyboardPressCount, 0);
        }

        private static IntPtr SetMouseHook(LowLevelMouseProc proc)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(null), 0);
        }

        private static IntPtr SetKeyboardHook(LowLevelKeyboardProc proc)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(null), 0);
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookMouseCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 && (MouseMessages)wParam == MouseMessages.WM_LBUTTONDOWN)
                {
                    Interlocked.Increment(ref _mouseClickCount);
                    LogCountsIfNeeded();
                }
            }
            catch { } // Ignore errors in the hook callback
            return CallNextHookEx(mouseHookID, nCode, wParam, lParam);
        }

        private static IntPtr HookKeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    Interlocked.Increment(ref _keyboardPressCount);
                    LogCountsIfNeeded();
                }
            }
            catch { } // Ignore errors in the hook callback
            return CallNextHookEx(keyboardHookID, nCode, wParam, lParam);
        }

        private static void LogCountsIfNeeded()
        {
            // Log counts only once per minute
            if ((DateTime.UtcNow - _lastLogTime).TotalMinutes >= 1)
            {
                Debug.WriteLine($"Current counts - Mouse: {MouseClickCount}, Keyboard: {KeyboardPressCount}");
                _lastLogTime = DateTime.UtcNow;
            }
        }

        private const int WM_KEYDOWN = 0x0100;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
} 