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
        // [2026-05-19 追加] メイン画面に戻す操作中のフラグ（Window_Closingでの二重処理を防ぐ）
        private bool _returningToMain = false;

        public TabWindow(MainInstance mainInstance)
        {
            InitializeComponent();
            this.mainInstance = mainInstance;
            mainInstance.mainGrid.Margin = new Thickness(5);
            Content = mainInstance;
            Title = (mainInstance.settings.TabNumber + 1).ToString();
            // [2026-05-19 変更] ボタンを無効化する代わりに「メインに戻す」に切り替える
            mainInstance.SetTabWindowMode(true, this);
        }

        // [2026-05-19 追加] メイン画面のタブに戻す（MainInstanceのボタンから呼び出される）
        public void ReturnToMain()
        {
            _returningToMain = true;
            MainWindow.GetMainWindowDataContext().ReturnTabToMain(mainInstance, this);
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // [2026-05-19 変更] メインに戻す操作の場合はReturnTabToMain側で処理済みのためスキップ
            if (_returningToMain)
            {
                return;
            }
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
