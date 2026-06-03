using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CaptureTool.Pages
{
    /// <summary>
    /// KeyBind.xaml の相互作用ロジック
    /// </summary>
    public partial class KeyBind : UserControl
    {
        public KeyBind()
        {
            InitializeComponent();
        }

        private Settings settings
        {
            get
            {
                if (DataContext is Settings dsettings)
                {
                    return dsettings;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool AutoResetHotKey { get; set; } = true;

        private void ClickStartSetting(object sender, RoutedEventArgs e)
        {
            const string inputText = "キーを入力";
            if (sender is Button button)
            {
                if (sender == keyButton)
                {
                    settings.KeyText = inputText;
                    settings.Key = ShowKeyInputDialog(false);

                    if (AutoResetHotKey)
                    {
                        settings.HotKeySettings.ResetWindowHotKey();
                    }
                }
                else if (sender == preKeyButton)
                {
                    settings.PreKeyText = inputText;
                    settings.PreKey = ShowKeyInputDialog(true);

                    if (AutoResetHotKey)
                    {
                        settings.HotKeySettings.ResetWindowHotKey();
                    }
                }
                else if (sender == screenKeyButton)
                {
                    settings.ScreenKeyText = inputText;
                    settings.ScreenKey = ShowKeyInputDialog(false);

                    if (AutoResetHotKey)
                    {
                        settings.HotKeySettings.ResetScreenHotKey();
                    }
                }
                else if (sender == screenPreKeyButton)
                {
                    settings.ScreenPreKeyText = inputText;
                    settings.ScreenPreKey = ShowKeyInputDialog(true);

                    if (AutoResetHotKey)
                    {
                        settings.HotKeySettings.ResetScreenHotKey();
                    }
                }
                else if (sender == selectKeyButton)
                {
                    settings.SelectKeyText = inputText;
                    settings.SelectKey = ShowKeyInputDialog(false);

                    if (AutoResetHotKey)
                    {
                        settings.HotKeySettings.ResetSelectHotKey();
                    }
                }
                else if (sender == selectPreKeyButton)
                {
                    settings.SelectPreKeyText = inputText;
                    settings.SelectPreKey = ShowKeyInputDialog(true);

                    if (AutoResetHotKey)
                    {
                        settings.HotKeySettings.ResetSelectHotKey();
                    }
                }
            }
        }

        private System.Windows.Forms.Keys ShowKeyInputDialog(bool enablePreMode)
        {
            Window activeWindow = MainWindow.ActiveWindow;
            // [変更] WinForms KeyInputForm → WPF KeyInputDialog に置換
            var dialog = new KeyInputDialog
            {
                Left = activeWindow.Left,
                Top = activeWindow.Top,
                Topmost = settings.TopMost,
                PreMode = enablePreMode
            };
            dialog.ShowDialog();
            // [変更] int（VKコード）→ Keys に変換（WinForms 境界はこのクラスで集約）
            return NormalizeVkToKeys(dialog.Key);
        }

        // [追加] VKコード(int) → Keys への変換
        // 修飾キーは左右の区別なく正規化する（表示・XML保存の一貫性を維持）
        private static System.Windows.Forms.Keys NormalizeVkToKeys(int vk)
        {
            switch (vk)
            {
                case VkShift:
                case VkLShift:
                case VkRShift: return System.Windows.Forms.Keys.Shift;
                case VkControl:
                case VkLControl:
                case VkRControl: return System.Windows.Forms.Keys.Control;
                case VkMenu:
                case VkLMenu:
                case VkRMenu: return System.Windows.Forms.Keys.Alt;
                default: return (System.Windows.Forms.Keys)vk;
            }
        }

        private const int VkShift = 0x10;
        private const int VkControl = 0x11;
        private const int VkMenu = 0x12;
        private const int VkLShift = 0xA0;
        private const int VkRShift = 0xA1;
        private const int VkLControl = 0xA2;
        private const int VkRControl = 0xA3;
        private const int VkLMenu = 0xA4;
        private const int VkRMenu = 0xA5;
    }
}
