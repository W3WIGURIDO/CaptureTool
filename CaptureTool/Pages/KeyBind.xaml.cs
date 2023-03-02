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
            KeyInputForm keyInputForm = new KeyInputForm()
            {
                StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                Location = new System.Drawing.Point((int)activeWindow.Left, (int)activeWindow.Top),
                TopMost = settings.TopMost
            };
            keyInputForm.PreMode = enablePreMode;
            keyInputForm.ShowDialog();
            return keyInputForm.Key;
        }
    }
}
