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
                    Visibility = Visibility.Hidden;
                    notifyIcon = new NotifyIconWrapper(this);
                }
            }
            else if (WindowState == WindowState.Normal)
            {
                if (notifyIcon != null)
                {
                    notifyIcon.Dispose();
                }
            }
        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindowDataContext.AddTab(mainWindowDataContext.LastTabNumber + 1);
        }

        private void mainTabCtl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mainWindowDataContext.RaisePropertyChanged(nameof(mainWindowDataContext.TopMost));
        }
    }
}
