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

namespace CaptureTool
{
    /// <summary>
    /// MainInstance.xaml の相互作用ロジック
    /// </summary>
    public partial class MainInstance : UserControl
    {
        public Settings settings;
        public MainInstance(int tabNumber)
        {
            settings = new Settings(tabNumber);
            InitializeComponent();
            this.DataContext = settings;
            settings.OwnerInstance = this;
            settings.HotKeySettings = new HotKeySettings(settings);
            settings.HotKeySettings.StartHotKey();
        }

        private void InvisibleButton_GotFocus(object sender, RoutedEventArgs e)
        {
            saveButton.Focus();
        }

        private void instanceSettingButton_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = new SettingWindow(settings, MainWindow.GetMainWindow());
            MainWindow.ActiveWindow = settingWindow;
            settingWindow.ShowDialog();
            MainWindow.ActiveWindow = MainWindow.GetMainWindow();
        }

        private void ClickSave(object sender, RoutedEventArgs e)
        {
            settings.SaveSettings();
            settings.HotKeySettings.ResetHotKey();
            OverlayDialog overlayDialog = new OverlayDialog("設定保存完了")
            {
                Owner = MainWindow.GetMainWindow()
            };
            overlayDialog.Show();
        }

        private void ClickReset(object sender, RoutedEventArgs e)
        {
            if (WpfFolderBrowser.CustomMessageBox.Show(MainWindow.GetMainWindow(), "設定をリセットします", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel, MessageBoxOptions.None) == MessageBoxResult.OK)
            {
                settings.ResetSettings();
            }
        }
    }
}
