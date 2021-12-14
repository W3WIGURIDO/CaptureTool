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
        private static MainWindow _ActiveWindow = null;
        public static MainWindow ActiveWindow { get => _ActiveWindow; }
        private HotKey windowHotKey;
        private HotKey screenHotKey;
        private HotKey selectHotKey;
        private Settings settings = new Settings();
        private NotifyIconWrapper notifyIcon;
        private static string WindowCapture = "WindowCapture";
        private static string ScreenCapture = "ScreenCapture";
        private static string SelectWindow = "SelectWindow";
        private bool loadFinished = false;
        private MiniWindow miniWindow;
        public IntPtr Handle { get; }
        private static MainWindow mainWindow;

        public static MainWindow GetMainWindow()
        {
            return mainWindow;
        }

        public MainWindow()
        {
            InitializeComponent();

            //System.Windows.Forms.Application.EnableVisualStyles();
            //System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            mainWindow = this;
            this.DataContext = settings;
            _ActiveWindow = this;
            Handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            windowHotKey.Dispose();
            if (settings.EnableAutoSave == true)
            {
                settings.SaveSettings();
            }
        }

        private void StartHotKey()
        {
            windowHotKey = new HotKey(EnumScan.FlagToMOD_KEY(settings.PreKey), settings.Key) { HotKeyName = WindowCapture };
            windowHotKey.HotKeyPush += new EventHandler(HotKey_HotKeyPush);
            screenHotKey = new HotKey(EnumScan.FlagToMOD_KEY(settings.ScreenPreKey), settings.ScreenKey) { HotKeyName = ScreenCapture };
            screenHotKey.HotKeyPush += new EventHandler(HotKey_HotKeyPush);
            selectHotKey = new HotKey(EnumScan.FlagToMOD_KEY(settings.SelectPreKey), settings.SelectKey) { HotKeyName = SelectWindow };
            selectHotKey.HotKeyPush += new EventHandler(HotKey_HotKeyPush);
        }

        private void ResetHotKey()
        {
            windowHotKey.Dispose();
            screenHotKey.Dispose();
            selectHotKey.Dispose();
            StartHotKey();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartHotKey();
            loadFinished = true;
        }

        private void HotKey_HotKeyPush(object sender, EventArgs e)
        {
            if (sender is HotKey tmpHotKey)
            {
                if (tmpHotKey.HotKeyName == SelectWindow)
                {
                    if (miniWindow != null)
                    {
                        miniWindow.WindowState = WindowState.Normal;
                        MainProcess.SwitchToThisWindow(miniWindow.Handle, true);
                        miniWindow.Activate();
                        MainProcess.SetWindowPos(miniWindow.Handle, MainProcess.HWND_TOPMOST, 0, 0, 0, 0, MainProcess.TOPMOST_FLAGS);

                        string cbtext = MainProcess.GetClipBoardText();
                        if (!string.IsNullOrEmpty(cbtext))
                        {
                            miniWindow.fileNameBox.Text = cbtext;
                        }

                        miniWindow.fileNameBox.Focus();
                        miniWindow.fileNameBox.SelectAll();
                        Task.Run(() =>
                        {
                            System.Threading.Thread.Sleep(100);
                            MainProcess.SetWindowPos(miniWindow.Handle, MainProcess.HWND_NOTOPMOST, 0, 0, 0, 0, MainProcess.NOTOPMOST_FLAGS);
                        });
                    }
                    else
                    {
                        Visibility = Visibility.Visible;
                        WindowState = WindowState.Normal;
                        MainProcess.SwitchToThisWindow(Handle, true);
                        Activate();
                        MainProcess.SetWindowPos(Handle, MainProcess.HWND_TOPMOST, 0, 0, 0, 0, MainProcess.TOPMOST_FLAGS);

                        string cbtext = MainProcess.GetClipBoardText();
                        if (!string.IsNullOrEmpty(cbtext))
                        {
                            fileNameBox.Text = cbtext;
                        }

                        fileNameBox.Focus();
                        fileNameBox.SelectAll();
                        Task.Run(() =>
                        {
                            System.Threading.Thread.Sleep(100);
                            MainProcess.SetWindowPos(Handle, MainProcess.HWND_NOTOPMOST, 0, 0, 0, 0, MainProcess.NOTOPMOST_FLAGS);
                        });
                    }
                }
                else
                {
                    Window targetWindow;
                    if (miniWindow != null)
                    {
                        targetWindow = miniWindow;
                    }
                    else
                    {
                        targetWindow = this;
                    }
                    if (tmpHotKey.HotKeyName == ScreenCapture)
                    {
                        Task.Run(() =>
                     {
                         targetWindow.Dispatcher.Invoke(() =>
                         {
                             targetWindow.Visibility = Visibility.Hidden;
                         }, System.Windows.Threading.DispatcherPriority.Send);
                     });
                    }
                    var positionSet = (PositionSet)positionSelect.SelectedValue;
                    string imageFormatSelectStr = imageFormatSelect.SelectedValue.ToString();
                    Task.Run(() =>
                    {
                        if (tmpHotKey.HotKeyName == ScreenCapture)
                        {
                            System.Threading.Thread.Sleep(200);
                            while (targetWindow.Visibility == Visibility.Visible)
                            {
                                System.Diagnostics.Debug.WriteLine("Sleep");
                                System.Threading.Thread.Sleep(200);
                            }
                        }
                        targetWindow.Dispatcher.Invoke(() =>
                        {
                            if (MainProcess.CaptureScreen(settings.Directory + "\\" + settings.SampleFileName, settings.Directory, imageFormatName: imageFormatSelectStr, overlayTime: settings.OverlayTimeInt, enableOverlay: settings.EnableOverlay == true, overlayHorizontalAlignment: positionSet.HorizontalAlignment, overlayVerticalAlignment: positionSet.VerticalAlignment, screenFlag: ScreenCapture.Equals(tmpHotKey.HotKeyName), aero: settings.EnableAero == true, enableCursor: settings.EnableCursor == true, captureMode: (int)captureModeSelect.SelectedValue, imageGridWidth: settings.OverlayX, imageGridHeight: settings.OverlayY, enableSetArrow: settings.EnableSetArrow == true, pixelFormat: (System.Drawing.Imaging.PixelFormat)pixelFormatSelect.SelectedValue))
                            {
                                settings.NumberCount++;
                                settings.NumberCountSave();
                            }
                        });
                        if (tmpHotKey.HotKeyName == ScreenCapture)
                        {
                            targetWindow.Dispatcher.Invoke(() =>
                            {
                                targetWindow.Visibility = Visibility.Visible;
                            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        }
                    });
                }
            }
        }

        private System.Windows.Forms.Keys ShowKeyInputDialog(bool enablePreMode)
        {
            KeyInputForm keyInputForm = new KeyInputForm()
            {
                StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                Location = new System.Drawing.Point((int)(Left), (int)(Top))
            };
            keyInputForm.PreMode = enablePreMode;
            keyInputForm.ShowDialog();
            return keyInputForm.Key;
        }

        private bool CheckModificationKey(System.Windows.Forms.Keys key)
        {
            if (key == System.Windows.Forms.Keys.LControlKey || key == System.Windows.Forms.Keys.RControlKey || key == System.Windows.Forms.Keys.Control || key == System.Windows.Forms.Keys.ControlKey || key == System.Windows.Forms.Keys.Alt ||
                key == System.Windows.Forms.Keys.LShiftKey || key == System.Windows.Forms.Keys.RShiftKey)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ClickStartSetting(object sender, RoutedEventArgs e)
        {
            const string inputText = "キーを入力";
            if (sender is Button button)
            {
                if (sender == keyButton)
                {
                    button.Content = inputText;
                    settings.Key = ShowKeyInputDialog(false);
                    Binding binding = new Binding("KeyText")
                    {
                        Source = settings
                    };
                    button.SetBinding(Button.ContentProperty, binding);
                }
                else if (sender == preKeyButton)
                {
                    button.Content = inputText;
                    settings.PreKey = ShowKeyInputDialog(true);
                    Binding binding = new Binding("PreKeyText")
                    {
                        Source = settings
                    };
                    button.SetBinding(Button.ContentProperty, binding);
                }
                else if (sender == screenKeyButton)
                {
                    button.Content = inputText;
                    settings.ScreenKey = ShowKeyInputDialog(false);
                    Binding binding = new Binding("ScreenKeyText")
                    {
                        Source = settings
                    };
                    button.SetBinding(Button.ContentProperty, binding);
                }
                else if (sender == screenPreKeyButton)
                {
                    button.Content = inputText;
                    settings.ScreenPreKey = ShowKeyInputDialog(true);
                    Binding binding = new Binding("ScreenPreKeyText")
                    {
                        Source = settings
                    };
                    button.SetBinding(Button.ContentProperty, binding);
                }
                else if (sender == selectKeyButton)
                {
                    button.Content = inputText;
                    settings.SelectKey = ShowKeyInputDialog(false);
                    Binding binding = new Binding("SelectKeyText")
                    {
                        Source = settings
                    };
                    button.SetBinding(Button.ContentProperty, binding);
                }
                else if (sender == selectPreKeyButton)
                {
                    button.Content = inputText;
                    settings.SelectPreKey = ShowKeyInputDialog(true);
                    Binding binding = new Binding("SelectPreKeyText")
                    {
                        Source = settings
                    };
                    button.SetBinding(Button.ContentProperty, binding);
                }
                ResetHotKey();
            }
        }

        private void ClickRef(object sender, RoutedEventArgs e)
        {
            WpfFolderBrowser.FolderDialogResult folderDialogResult = WpfFolderBrowser.Main.ShowFolderDialog(this, false, baseAddress: settings.Directory);
            if (folderDialogResult != null)
            {
                settings.Directory = folderDialogResult.FullName;
            }
        }

        private void ClickSave(object sender, RoutedEventArgs e)
        {
            settings.SaveSettings();
            ResetHotKey();
            OverlayDialog overlayDialog = new OverlayDialog("設定保存完了")
            {
                Owner = this
            };
            overlayDialog.Show();
        }

        private void ClickReset(object sender, RoutedEventArgs e)
        {
            if (WpfFolderBrowser.CustomMessageBox.Show(this, "設定をリセットします", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel, MessageBoxOptions.None) == MessageBoxResult.OK)
            {
                settings.ResetSettings();
            }
        }

        private void NumberResetClick(object sender, RoutedEventArgs e)
        {
            settings.NumberCount = 0;
            settings.NumberCountSave();
        }

        private void InvisibleButton_GotFocus(object sender, RoutedEventArgs e)
        {
            saveButton.Focus();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                if (settings.EnableTray == true)
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

        private void FileNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (loadFinished)
            {
                settings.FileName = fileNameBox.Text;
                settings.NumberCount = 0;
            }
            MainProcess.CreateFileNameNumberCountButtons(settings.FileName, countButtonPanel, settings);
        }

        private void SaveFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.Directory = saveFolder.Text;
        }

        private void DigitsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.DigitsText = digitsTextBox.Text;
        }

        private void OverlayTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.OverlayTime = overlayTimeTextBox.Text;
        }

        private void CountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(countTextBox.Text, out int result))
            {
                settings.NumberCount = result;
            }
            else
            {
                countTextBox.Text = 0.ToString();
            }
        }

        private void CountUpButton_Click(object sender, RoutedEventArgs e)
        {
            settings.NumberCount++;
        }

        private void CountDownButton_Click(object sender, RoutedEventArgs e)
        {
            settings.NumberCount--;
        }

        private void MiniModeCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (miniModeCheck.IsChecked == true)
            {
                miniWindow = new MiniWindow(settings) { Left = this.Left, Top = this.Top };
                Visibility = Visibility.Hidden;
                miniModeCheck.IsChecked = false;
                miniWindow.Show();
            }
        }

        public void ReturnFromMiniMode()
        {
            miniWindow = null;
            Visibility = Visibility.Visible;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            /*
            if (sender is Window windowVisibleChanged)
            {
                if (windowVisibleChanged.Visibility == Visibility.Hidden)
                {
                    var sb = new System.Windows.Media.Animation.Storyboard();
                    var da = new System.Windows.Media.Animation.DoubleAnimation();
                    System.Windows.Media.Animation.Storyboard.SetTarget(da, this);
                    System.Windows.Media.Animation.Storyboard.SetTargetProperty(da, new PropertyPath("(Window.Opacity)"));
                    da.To = 0;
                    da.Duration = TimeSpan.FromMilliseconds(1);
                    sb.Children.Add(da);
                    sb.Begin();
                }
            }
            */
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            settings.NumberCount = MainProcess.GetContinueFileName(settings);
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("Explorer", settings.Directory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void ParentFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string tmpDir = settings.Directory;
            if (tmpDir.Last() == '\\')
            {
                tmpDir = tmpDir.Substring(0, tmpDir.Length - 1);
            }
            string[] enSplited = tmpDir.Split('\\');
            if (enSplited.Length > 1)
            {
                settings.Directory = string.Join("\\", enSplited.Take(enSplited.Length - 1));
            }
        }
    }
}
