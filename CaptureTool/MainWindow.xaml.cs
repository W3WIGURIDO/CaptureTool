using Microsoft.WindowsAPICodePack.Dialogs;
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
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Window _ActiveWindow = null;
        public static Window ActiveWindow { get => _ActiveWindow; set { _ActiveWindow = value; } }
        public static TextBox ActiveFileNameBox { get; set; }
        private NotifyIconWrapper notifyIcon;
        private System.Windows.Interop.WindowInteropHelper _InteropHelper;
        public IntPtr Handle { get => _InteropHelper.Handle; }
        private static MainWindow mainWindow;
        private static MainWindowDataContext mainWindowDataContext;
        public static Logger logger;

        public static MainWindow GetMainWindow()
        {
            return mainWindow;
        }

        public static MainWindowDataContext GetMainWindowDataContext()
        {
            return mainWindowDataContext;
        }

        public MainWindow()
        {
            InitializeComponent();

            mainWindow = this;
            _ActiveWindow = this;
            _InteropHelper = new System.Windows.Interop.WindowInteropHelper(this);
            mainWindowDataContext = new MainWindowDataContext();
            mainWindowDataContext.AddTab(0);
            mainWindowDataContext.RefFolderCom.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(mainWindowDataContext.RefFolderCom, ClickRefCall));
            DataContext = mainWindowDataContext;
            logger = new Logger(AppDomain.CurrentDomain.BaseDirectory + "log.txt", 0);
        }

        public void ClickRefCall(object sender, RoutedEventArgs e)
        {
            mainWindowDataContext.UserControls[mainWindowDataContext.TabSelectedIndex].saveDirGrid.ClickRef(sender, e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (MainInstance mainInstance in mainWindowDataContext.UserControls)
            {
                mainInstance.settings.HotKeySettings.DisposeHotKeys();
                if (mainInstance.settings.EnableAutoSave == true)
                {
                    mainInstance.settings.SaveSettings();
                }
            }
            logger?.Dispose();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                if (mainWindowDataContext.UserControls[mainTabCtl.SelectedIndex].settings.EnableTray == true)
                {
                    // [2026-05-11 修正] 既存のnotifyIconをDisposeしてから新規作成（二重生成防止）
                    if (notifyIcon != null)
                    {
                        notifyIcon.Dispose();
                        notifyIcon = null;
                    }
                    Visibility = Visibility.Hidden;
                    notifyIcon = new NotifyIconWrapper(this);
                }
            }
            else if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized)
            {
                // [2026-05-11 修正] スリープ復帰・モニター再接続時にOSが自動でウィンドウを
                // 復元した際、Visibilityが Hidden のまま残りプロセスのみ残存する問題を修正
                if (Visibility != Visibility.Visible)
                {
                    Visibility = Visibility.Visible;
                }
                if (notifyIcon != null)
                {
                    notifyIcon.Dispose();
                    notifyIcon = null;
                }
            }
        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            // [2026-05-11 変更] 常に末尾番号+1ではなく、最小の未使用番号でタブを開く
            mainWindowDataContext.AddTab(mainWindowDataContext.GetNextTabNumber());
        }

        private void mainTabCtl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mainWindowDataContext.RaisePropertyChanged(nameof(mainWindowDataContext.TopMost));
        }
    }
}
