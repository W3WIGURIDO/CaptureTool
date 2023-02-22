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
            windowHotKey = new HotKey(EnumScan.FlagToMOD_KEY(settings.PreKey), settings.Key) { HotKeyName = WindowCapture };
            windowHotKey.HotKeyPush += new EventHandler(HotKey_HotKeyPush);
            screenHotKey = new HotKey(EnumScan.FlagToMOD_KEY(settings.ScreenPreKey), settings.ScreenKey) { HotKeyName = ScreenCapture };
            screenHotKey.HotKeyPush += new EventHandler(HotKey_HotKeyPush);
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
            windowHotKey.Dispose();
            screenHotKey.Dispose();
            selectHotKey.Dispose();
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
                    var positionSet = settings.ViewPosition[settings.ViewPosition.Keys.ElementAt(settings.PositionIndex)];
                    string imageFormatSelectStr = settings.SaveFormats[settings.SaveFormats.Keys.ElementAt(settings.SaveFormatIndex)];
                    Task.Run(() =>
                    {
                        if (visibilityControl)
                        {
                            System.Threading.Thread.Sleep(200);
                        }
                        targetWindow.Dispatcher.Invoke(() =>
                        {
                            string compressOption = "";
                            if (settings.CompressSelect == CompressType.Optipng)
                            {
                                compressOption = settings.CompressNums.Keys.ElementAt(settings.CompressIndex);
                            }
                            else if (settings.CompressSelect == CompressType.Zopfli)
                            {
                                compressOption = settings.CompressNumsZopfli.Keys.ElementAt(settings.CompressIndexZopfli);
                            }
                            if (MainProcess.CaptureScreen(settings.Directory + "\\" + settings.SampleFileName, settings.Directory, imageFormatName: imageFormatSelectStr, overlayTime: settings.OverlayTimeInt, enableOverlay: settings.EnableOverlay == true, overlayHorizontalAlignment: positionSet.HorizontalAlignment, overlayVerticalAlignment: positionSet.VerticalAlignment, screenFlag: ScreenCapture.Equals(tmpHotKey.HotKeyName), aero: settings.EnableAero == true, enableCursor: settings.EnableCursor == true, captureMode: settings.CaptureModeIndex, imageGridWidth: settings.OverlayX, imageGridHeight: settings.OverlayY, enableSetArrow: settings.EnableSetArrow == true, pixelFormat: settings.PixelFormats.Keys.ElementAt(settings.PixelFormatIndex), compressMode: (int)settings.CompressSelect, compressOption: compressOption))
                            {
                                settings.NumberCount++;
                            }
                        });
                        if (visibilityControl)
                        {
                            targetWindow.Dispatcher.Invoke(() =>
                            {
                                targetWindow.Visibility = Visibility.Visible;
                            }, System.Windows.Threading.DispatcherPriority.Background);
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
