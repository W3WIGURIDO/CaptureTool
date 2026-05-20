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
        public System.Windows.Controls.TabItem tabItem;

        // [2026-05-19 追加] 別ウィンドウ表示中かどうかのフラグと親TabWindowの参照
        private bool _isInTabWindow = false;
        private TabWindow _ownerTabWindow = null;

        public MainInstance(int tabNumber)
        {
            settings = new Settings(tabNumber);
            InitializeComponent();
            this.DataContext = settings;
            settings.OwnerInstance = this;
            settings.HotKeySettings = new HotKeySettings(settings);
        }

        // [2026-05-19 追加] 別ウィンドウ表示モードを切り替える（ボタン表示・動作の切り替え）
        public void SetTabWindowMode(bool isInTabWindow, TabWindow ownerTabWindow = null)
        {
            _isInTabWindow = isInTabWindow;
            _ownerTabWindow = ownerTabWindow;
            viewWindowButton.Content = isInTabWindow ? "メインに戻す" : "別ウィンドウで表示";
            viewWindowButton.IsEnabled = true;
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
            // [2026-05-19 変更] 別ウィンドウ表示中は「メインに戻す」動作に切り替える
            if (_isInTabWindow && _ownerTabWindow != null)
            {
                _ownerTabWindow.ReturnToMain();
            }
            else
            {
                MainWindow.GetMainWindowDataContext().ViewWindowTab();
            }
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

        // [2026-05-19 追加] キャプチャ履歴ウィンドウを開く（既に開いている場合は前面に出す）
        private void historyButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryWindow.ShowOrActivate(settings.TopMost);
        }
    }
}
