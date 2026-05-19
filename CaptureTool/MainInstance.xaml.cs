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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace CaptureTool
{
    /// <summary>
    /// MainInstance.xaml の相互作用ロジック
    /// </summary>
    public partial class MainInstance : UserControl
    {
        public Settings settings;
        public System.Windows.Controls.TabItem tabItem; //閉じる際にTabItem参照用

        public MainInstance(int tabNumber)
        {
            settings = new Settings(tabNumber);
            InitializeComponent();
            this.DataContext = settings;
            settings.OwnerInstance = this;
            settings.HotKeySettings = new HotKeySettings(settings);
            //settings.HotKeySettings.StartHotKey();
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

        private void viewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetMainWindowDataContext().ViewWindowTab();
        }

        private void closeTabButton_Click(object sender, RoutedEventArgs e)
        {
            // [2026-05-19 修正] TabWindowに表示されている場合はTabWindow自体を閉じる
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is TabWindow tabWindow)
            {
                tabWindow.Close();
            }
            else
            {
                MainWindow.GetMainWindowDataContext().RemoveTab(this, tabItem);
            }
        }
    }
}
