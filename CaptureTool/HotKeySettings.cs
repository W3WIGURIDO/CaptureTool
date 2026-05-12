using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CaptureTool
{
    public class HotKeySettings
    {
        private Settings settings;
        public HotKeySettings(Settings settings)
        {
            this.settings = settings;
        }

        private HotKey windowHotKey;
        private HotKey screenHotKey;
        private HotKey selectHotKey;

        private static readonly string WindowCapture = "WindowCapture";
        private static readonly string ScreenCapture = "ScreenCapture";
        private static readonly string SelectWindow = "SelectWindow";

        public void StartHotKey()
        {
            if (settings.WindowCaptureEnabled)
            {
                StartWindowHotkKey();
            }
            if (settings.ScreenCaptureEnabled)
            {
                StartScreenHotKey();
            }
            if (settings.SelectEnabled)
            {
                StartSelectHotKey();
            }
        }

        public void StartWindowHotkKey()
        {
            windowHotKey = new HotKey(EnumScan.FlagToMOD_KEY(settings.PreKey), settings.Key) { HotKeyName = WindowCapture };
            windowHotKey.HotKeyPush += new EventHandler(HotKey_HotKeyPush);
        }

        public void StartScreenHotKey()
        {
            screenHotKey = new HotKey(EnumScan.FlagToMOD_KEY(settings.ScreenPreKey), settings.ScreenKey) { HotKeyName = ScreenCapture };
            screenHotKey.HotKeyPush += new EventHandler(HotKey_HotKeyPush);
        }

        public void StartSelectHotKey()
        {
            selectHotKey = new HotKey(EnumScan.FlagToMOD_KEY(settings.SelectPreKey), settings.SelectKey) { HotKeyName = SelectWindow };
            selectHotKey.HotKeyPush += new EventHandler(HotKey_SelectWindow);
        }

        public void ResetHotKey()
        {
            DisposeHotKeys();
            StartHotKey();
        }

        public void DisposeHotKeys()
        {
            windowHotKey?.TryDispose();
            screenHotKey?.TryDispose();
            selectHotKey?.TryDispose();
        }

        public void ResetWindowHotKey()
        {
            windowHotKey?.TryDispose();
            if (settings.WindowCaptureEnabled)
            {
                StartWindowHotkKey();
            }
        }

        public void ResetScreenHotKey()
        {
            screenHotKey?.TryDispose();
            if (settings.ScreenCaptureEnabled)
            {
                StartScreenHotKey();
            }
        }

        public void ResetSelectHotKey()
        {
            selectHotKey?.TryDispose();
            if (settings.SelectEnabled)
            {
                StartSelectHotKey();
            }
        }

        public void HotKey_HotKeyPush(object sender, EventArgs e)
        {
            MainWindow targetWindow = MainWindow.GetMainWindow();
            IntPtr targetHandle = targetWindow.Handle;
            try
            {
                if (sender is HotKey tmpHotKey)
                {
                    bool visibilityControl = tmpHotKey.HotKeyName == ScreenCapture && settings.EnableVisibilityControl == true;
                    if (visibilityControl)
                    {
                        Task.Run(() =>
                        {
                            targetWindow.Dispatcher.Invoke(() =>
                            {
                                targetWindow.Visibility = Visibility.Hidden;
                            }, System.Windows.Threading.DispatcherPriority.Send);
                        });
                    }
                    Task.Run(() =>
                    {
                        if (visibilityControl)
                        {
                            System.Threading.Thread.Sleep(200);
                        }
                        // [2026-05-12 修正] Task.Run内の例外はtry-catchで補足されないため、
                        // visibilityControlが有効な場合にDispatcher.Invoke失敗等で
                        // Visibility = Visibleへの復元が行われずウィンドウが消失する問題を修正
                        try
                        {
                            targetWindow.Dispatcher.Invoke(() =>
                            {
                                if (MainProcess.CaptureScreen(settings, screenFlag: ScreenCapture.Equals(tmpHotKey.HotKeyName)))
                                {
                                    settings.NumberCount++;
                                }
                            });
                        }
                        finally
                        {
                            if (visibilityControl)
                            {
                                try
                                {
                                    targetWindow.Dispatcher.Invoke(() =>
                                    {
                                        targetWindow.Visibility = Visibility.Visible;
                                    }, System.Windows.Threading.DispatcherPriority.Background);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine("ウィンドウ再表示失敗: " + ex.Message);
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                WpfFolderBrowser.CustomMessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public void HotKey_SelectWindow(object sender, EventArgs e)
        {
            MainWindow targetWindow = MainWindow.GetMainWindow();
            TextBox targetBox = settings.OwnerInstance.saveDirGrid.fileNameBox;
            try
            {
                targetWindow.Visibility = Visibility.Visible;
                targetWindow.WindowState = WindowState.Normal;
                targetWindow.Activate();

                string cbtext = MainProcess.GetClipBoardText();
                if (!string.IsNullOrEmpty(cbtext))
                {
                    targetBox.Text = cbtext;
                }

                targetBox.Focus();
                targetBox.SelectAll();
            }
            catch (Exception ex)
            {
                WpfFolderBrowser.CustomMessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}
