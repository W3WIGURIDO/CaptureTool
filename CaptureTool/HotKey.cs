using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Interop;

namespace CaptureTool
{
    class HotKey : IDisposable
    {
        // [変更] WinForms HotKeyForm → HwndSource ベースの HotKeyWindow に置換
        private HotKeyWindow _window;
        public event EventHandler HotKeyPush;
        public string HotKeyName { get; set; }

        private bool _Disposed = false;
        public bool Disposed => _Disposed;

        public HotKey(MOD_KEY modKey, Keys key)
        {
            _window = new HotKeyWindow(modKey, key, raiseHotKeyPush);
        }

        private void raiseHotKeyPush()
        {
            HotKeyPush?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _window.Dispose();
            _Disposed = true;
        }

        public bool TryDispose()
        {
            if (!Disposed)
            {
                Dispose();
                return true;
            }
            return false;
        }

        // [変更] WinForms Form を HwndSource（HWND_MESSAGE ウィンドウ）に置換
        // WM_HOTKEY を受信するための非表示メッセージウィンドウ
        private class HotKeyWindow : IDisposable
        {
            [DllImport("user32.dll")]
            static extern int RegisterHotKey(IntPtr hWnd, int id, MOD_KEY fsModifiers, Keys vk);

            [DllImport("user32.dll")]
            static extern int UnregisterHotKey(IntPtr hWnd, int id);

            const int WM_HOTKEY = 0x0312;

            private readonly HwndSource _hwndSource;
            private readonly HwndSourceHook _hook;
            private readonly ThreadStart _proc;
            private int _id;

            public HotKeyWindow(MOD_KEY modKey, Keys key, ThreadStart proc)
            {
                _proc = proc;
                _hook = WndProc;

                // HWND_MESSAGE（-3）を親に指定することで非表示のメッセージ専用ウィンドウを生成する
                var parameters = new HwndSourceParameters("HotKeyWindow")
                {
                    WindowStyle = 0,
                    ExtendedWindowStyle = 0,
                    PositionX = 0,
                    PositionY = 0,
                    Width = 0,
                    Height = 0,
                    ParentWindow = new IntPtr(-3),
                };
                _hwndSource = new HwndSource(parameters);
                _hwndSource.AddHook(_hook);

                for (int i = 0x0000; i <= 0xbfff; i++)
                {
                    if (RegisterHotKey(_hwndSource.Handle, i, modKey, key) != 0)
                    {
                        _id = i;
                        break;
                    }
                }
            }

            private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                if (msg == WM_HOTKEY && wParam.ToInt32() == _id)
                {
                    _proc();
                    handled = true;
                }
                return IntPtr.Zero;
            }

            public void Dispose()
            {
                UnregisterHotKey(_hwndSource.Handle, _id);
                _hwndSource.RemoveHook(_hook);
                _hwndSource.Dispose();
            }
        }
    }

    public enum MOD_KEY : int
    {
        ALT = 0x0001,
        CONTROL = 0x0002,
        SHIFT = 0x0004,
    }

    public static class EnumScan
    {
        public static Keys[] GetKeyModFlags(Keys key)
        {
            List<Keys> keyList = new System.Collections.Generic.List<Keys>();
            void FlagCheck(Keys flagKey)
            {
                if (key.HasFlag(flagKey))
                {
                    keyList.Add(flagKey);
                }
            }

            FlagCheck(Keys.LControlKey);
            FlagCheck(Keys.RControlKey);
            FlagCheck(Keys.Control);
            FlagCheck(Keys.ControlKey);
            FlagCheck(Keys.Alt);
            FlagCheck(Keys.LShiftKey);
            FlagCheck(Keys.RShiftKey);
            FlagCheck(Keys.ShiftKey);
            FlagCheck(Keys.Shift);

            return keyList.ToArray();
        }

        public static MOD_KEY FlagToMOD_KEY(Keys key)
        {
            Keys[] flagKeys = GetKeyModFlags(key);
            if (flagKeys.Contains(Keys.LControlKey) || flagKeys.Contains(Keys.RControlKey) || flagKeys.Contains(Keys.Control) || flagKeys.Contains(Keys.ControlKey))
            {
                return MOD_KEY.CONTROL;
            }
            else if (flagKeys.Contains(Keys.Alt))
            {
                return MOD_KEY.ALT;
            }
            else if (flagKeys.Contains(Keys.LShiftKey) || flagKeys.Contains(Keys.RShiftKey) || flagKeys.Contains(Keys.ShiftKey) || flagKeys.Contains(Keys.Shift))
            {
                return MOD_KEY.SHIFT;
            }
            else
            {
                return MOD_KEY.CONTROL;
            }
        }
    }
}