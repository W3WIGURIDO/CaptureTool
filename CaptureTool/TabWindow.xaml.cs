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
using System.Windows.Shapes;

namespace CaptureTool
{
    /// <summary>
    /// TabWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TabWindow : Window
    {
        private MainInstance mainInstance;
        public TabWindow(MainInstance mainInstance)
        {
            InitializeComponent();
            this.mainInstance = mainInstance;
            mainInstance.mainGrid.Margin = new Thickness(5);
            Content = mainInstance;
            Title = (mainInstance.settings.TabNumber + 1).ToString();
            mainInstance.viewWindowButton.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // [2026-05-19 修正] TabWindowsリストから自身を削除し、設定保存・終了判定を行う
            var ctx = MainWindow.GetMainWindowDataContext();
            ctx.UserControls.Remove(mainInstance);
            ctx.TabWindows.Remove(this);
            mainInstance.settings.HotKeySettings.DisposeHotKeys();
            if (mainInstance.settings.EnableAutoSave == true)
            {
                mainInstance.settings.SaveSettings();
            }
            // [2026-05-19 追加] 全タブが閉じられた場合はアプリを終了する
            ctx.CheckAndExitIfEmpty();
        }
    }
}
