using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CaptureTool
{
    public partial class KeyInputDialog : Window
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12;
        private const int VK_LSHIFT = 0xA0;
        private const int VK_RSHIFT = 0xA1;
        private const int VK_LCONTROL = 0xA2;
        private const int VK_RCONTROL = 0xA3;
        private const int VK_LMENU = 0xA4;
        private const int VK_RMENU = 0xA5;
        // [追加] Windows キー：スタートメニュー競合防止のため除外対象
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn,
                                              IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                                            IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("imm32.dll")]
        static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

        public bool PreMode { get; set; } = false;
        public int Key { get; private set; }

        private LowLevelKeyboardProc _hookProc;
        private IntPtr _hookHandle = IntPtr.Zero;
        private bool _keyCaptured = false;

        public KeyInputDialog()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            ImmAssociateContext(hwnd, IntPtr.Zero);

            _hookProc = KeyboardHookCallback;
            _hookHandle = SetWindowsHookEx(
                WH_KEYBOARD_LL, _hookProc, GetModuleHandle(null), 0);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }
            base.OnClosed(e);
        }

        private IntPtr KeyboardHookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                // [修正①] ToInt32() → long キャストで 64bit OverflowException を回避
                long msg = (long)wParam;

                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    // [修正②] キャプチャ済みなら全入力をブロック
                    // BeginInvoke で Close が実行されるまでのリピート入力を遮断する
                    if (_keyCaptured)
                        return new IntPtr(1);

                    var kbStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                    int vk = (int)kbStruct.vkCode;

                    // [修正③] Windows キーは常にブロックして除外
                    // スタートメニューとの競合防止、修飾キーとしても非対応のため
                    if (IsExcludedVk(vk))
                        return new IntPtr(1);

                    bool isMod = IsModifierVk(vk);
                    bool captured = false;

                    if (PreMode && isMod)
                    {
                        // 修飾キーのキャプチャ（Alt は WM_SYSKEYDOWN で届く）
                        Key = vk;
                        captured = true;
                    }
                    else if (!PreMode && !isMod && msg == WM_KEYDOWN)
                    {
                        // 通常キーのキャプチャ（Alt 組み合わせは除外）
                        Key = vk;
                        captured = true;
                    }

                    if (captured)
                    {
                        _keyCaptured = true;
                        Dispatcher.BeginInvoke(new Action(Close));
                        return new IntPtr(1);
                    }
                }
            }
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private static bool IsModifierVk(int vk)
        {
            return vk == VK_SHIFT || vk == VK_LSHIFT || vk == VK_RSHIFT
                || vk == VK_CONTROL || vk == VK_LCONTROL || vk == VK_RCONTROL
                || vk == VK_MENU || vk == VK_LMENU || vk == VK_RMENU;
        }

        // [追加] 登録対象から除外するキー（捕捉せず常にブロック）
        private static bool IsExcludedVk(int vk)
        {
            return vk == VK_LWIN || vk == VK_RWIN;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Key = 0;
            Close();
        }
    }
}