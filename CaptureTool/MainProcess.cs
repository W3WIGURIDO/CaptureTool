using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Drawing;
using System.Windows;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CaptureTool
{
    public static class MainProcess
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_SHOWWINDOW = 0x0040;
        public const uint TOPMOST_FLAGS = (SWP_NOSIZE | SWP_NOMOVE);
        public const uint NOTOPMOST_FLAGS = (SWP_SHOWWINDOW | SWP_NOSIZE | SWP_NOMOVE);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        const int ASYNCWINDOWPOS = 0x4000;

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private extern static bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, out RECT lpRect, int attrSize);

        public enum DWMWINDOWATTRIBUTE : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCpaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation,
            PlaceHolder1,
            PlaceHolder2,
            PlaceHolder3,
            AccentPolicy = 19
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public extern static bool SetProcessDPIAware();

        [DllImport("user32.dll")]
        public extern static IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        public extern static IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        public extern static int GetDeviceCaps(IntPtr hdc, int index);

        [DllImport("user32.dll")]
        public extern static int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        public static int SM_CXSCREEN = 0;
        public static int SM_CYSCREEN = 1;
        public static int SM_CXFULLSCREEN = 16;
        public static int SM_CYFULLSCREEN = 17;
        public static int SM_XVIRTUALSCREEN = 76;
        public static int SM_YVIRTUALSCREEN = 77;
        public static int SM_CXVIRTUALSCREEN = 78;
        public static int SM_CYVIRTUALSCREEN = 79;

        [DllImport("user32.dll")]
        public extern static int GetSystemMetrics(int smIndex);

        [DllImport("user32.dll")]
        public static extern bool GetCursorInfo(out CURSORINFO pci);

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        public struct POINT
        {
            public int X;
            public int Y;
        }

        public const uint CURSOR_SHOWING = 0x00000001;

        [DllImport("user32.dll")]
        public static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO pIconInfo);

        public struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("gdi32.dll")]
        public static extern int BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hsrcDC, int xSrc, int ySrc, int dwRop);

        public const int SRCCOPY = 13369376;

        /**  
        <summary>
        ハンドル
        表示状態
        直前にウィンドウが表示されている場合, 0以外、そうでない場合は0
        </summary>
        **/
        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr handle, int command);

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public int cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public int dwStyle;
            public int dwExStyle;
            public int dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public short atomWindowType;
            public short wCreatorVersion;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Windows.Point ptMinPosition;
            public System.Windows.Point ptMaxPosition;
            public Rectangle rcNormalPosition;
        }


        static MainProcess()
        {
            AllScreens = System.Windows.Forms.Screen.AllScreens.ToList();
            AllScreens.Remove(System.Windows.Forms.Screen.PrimaryScreen);
            AllScreens.Insert(0, System.Windows.Forms.Screen.PrimaryScreen);
        }

        public static string FileNameDateRegexConvert(string origStr)
        {
            const string WordDate = "<Date>";
            const string WordTime = "<Time>";
            string tmpStr = origStr;
            if (origStr.Contains(WordDate))
            {
                string nowYMD = DateTime.Now.ToString("yyyyMMdd");
                tmpStr = tmpStr.Replace(WordDate, nowYMD);
            }
            if (origStr.Contains(WordTime))
            {
                string nowHMS = DateTime.Now.ToString("HHmmss");
                tmpStr = tmpStr.Replace(WordTime, nowHMS);
            }
            return tmpStr;
        }

        public static string FileNameDirRegexConvert(string origStr, string dirName)
        {
            const string WordDir = "<Dir>";
            string tmpStr = origStr;
            if (origStr.Contains(WordDir))
            {
                string dirNameLast = Path.GetFileName(dirName);
                tmpStr = tmpStr.Replace(WordDir, dirNameLast);
            }
            return tmpStr;
        }


        public static string FileNameWindowTitleRegexConvert(string origStr, string title)
        {
            const string WordWindowTitle = "<WindowTitle>";
            const string DesktopText = "Desktop";
            string tmpStr = origStr;
            if (origStr.Contains(WordWindowTitle))
            {
                if (string.IsNullOrEmpty(title))
                {
                    title = DesktopText;
                }
                title = System.Text.RegularExpressions.Regex.Replace(title, "[\\\\/:*?\"<>|]", string.Empty);
                tmpStr = tmpStr.Replace(WordWindowTitle, title);
            }
            return tmpStr;
        }

        public static string doWindowTitleReplace(string origStr, IEnumerable<Settings.WindowTitleReplaceSetting> list)
        {
            string tmpStr = origStr;
            foreach (Settings.WindowTitleReplaceSetting setting in list)
            {
                try
                {
                    if (setting.isRegex == true)
                    {
                        tmpStr = System.Text.RegularExpressions.Regex.Replace(tmpStr, setting.pattern, setting.replacement);
                    }
                    else
                    {
                        tmpStr = tmpStr.Replace(setting.pattern, setting.replacement);
                    }
                }
                catch (Exception ex)
                {
                    OverlayDialog overlayDialog = new OverlayDialog(ex.Message)
                    {
                        Owner = MainWindow.GetMainWindow()
                    };
                    overlayDialog.Show();
                    MainWindow.logger.Error(ex.Message);
                    Debug.WriteLine(ex.Message);
                }
            }
            return tmpStr;
        }

        public static string FileNameHostNameRegexConvert(string origStr)
        {
            const string WordHostName = "<HostName>";
            string tmpStr = origStr;
            if (origStr.Contains(WordHostName))
            {
                try
                {
                    string hostname = System.Net.Dns.GetHostName();
                    hostname = System.Text.RegularExpressions.Regex.Replace(hostname, "[\\\\/:*?\"<>|]", string.Empty);
                    tmpStr = tmpStr.Replace(WordHostName, hostname);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return tmpStr;
        }

        public static string doWindowTitleReplaceHosts(string origStr, Dictionary<string, List<string>> hostsData, int replaceMode)
        {
            string tmpStr = origStr;
            var mc = System.Text.RegularExpressions.Regex.Matches(tmpStr, "[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+");
            foreach (Match m in mc)
            {
                if (hostsData.ContainsKey(m.Value))
                {
                    string replaceText;
                    switch (replaceMode)
                    {
                        case 1:
                            replaceText = hostsData[m.Value][0] + "(" + m.Value + ")";
                            break;
                        default:
                            replaceText = hostsData[m.Value][0];
                            break;
                    }
                    tmpStr = tmpStr.Replace(m.Value, replaceText);
                }
            }
            return tmpStr;
        }

        public static string FileNameWindowTitleRegexConvert(string origStr, bool screenFlag, IntPtr windowHandle, Settings settings)
        {
            const string WordWindowTitle = "<WindowTitle>";
            const string DesktopText = "Desktop";
            string tmpStr = origStr;

            if (tmpStr.Contains(WordWindowTitle))
            {
                if (screenFlag)
                {
                    tmpStr = tmpStr.Replace(WordWindowTitle, DesktopText);
                }
                else
                {
                    Process process = GetProcessFromHandle(windowHandle);
                    string title = process.MainWindowTitle;
                    if (string.IsNullOrEmpty(title))
                    {
                        title = DesktopText;
                    }
                    title = doWindowTitleReplaceHosts(title, settings.HostsData, settings.IpHostTransSettingIndex);
                    title = System.Text.RegularExpressions.Regex.Replace(title, "[\\\\/:*?\"<>|]", string.Empty);
                    title = doWindowTitleReplace(title, settings.WindowTitleReplaceSettings);
                    tmpStr = tmpStr.Replace(WordWindowTitle, title);
                }
            }
            return tmpStr;
        }

        public static Tuple<string, string> doNameRegexConverts(string fileName, string dirName, bool screenFlag, IntPtr windowHandle, Settings settings)
        {
            fileName = FileNameHostNameRegexConvert(fileName);
            dirName = FileNameHostNameRegexConvert(dirName);

            fileName = FileNameWindowTitleRegexConvert(fileName, screenFlag, windowHandle, settings);
            dirName = FileNameWindowTitleRegexConvert(dirName, screenFlag, windowHandle, settings);

            fileName = FileNameDateRegexConvert(fileName);
            dirName = FileNameDateRegexConvert(dirName);

            dirName = System.Text.RegularExpressions.Regex.Replace(dirName, "[/*?\"<>|]", string.Empty);                //フォルダ名確定

            fileName = FileNameDirRegexConvert(fileName, dirName);

            fileName = System.Text.RegularExpressions.Regex.Replace(fileName, "[\\\\:/*?\"<>|]", string.Empty);                //ファイル名確定
            return new Tuple<string, string>(fileName, dirName);
        }

        public static Process GetProcessFromHandle(IntPtr windowHandle)
        {
            Process process = null;
            if (process == null)
            {
                try
                {
                    GetWindowThreadProcessId(windowHandle, out int processID);
                    process = Process.GetProcessById(processID);
                    return process;
                }
                catch (Exception pex)
                {
                    Console.WriteLine(pex.Message);
                }
            }
            return null;
        }

        public static OverlayWindow prevOverlayWindow;
        private const string FormatText = "Text";
        private static string tempPath = Path.GetTempPath();
        private static readonly string callCompressPath = AppDomain.CurrentDomain.BaseDirectory + "CallCompress.exe";
        private static readonly string optipngPath = AppDomain.CurrentDomain.BaseDirectory + "optipng.exe";
        private static readonly string zopflipngPath = AppDomain.CurrentDomain.BaseDirectory + "zopflipng.exe";

        public static bool CaptureScreen(Settings settings, bool screenFlag = true)
        {
            string fileName = settings.FileName;
            string dirName = settings.Directory;
            string imageFormatName = settings.SaveFormats[settings.SaveFormats.Keys.ElementAt(settings.SaveFormatIndex)];
            int overlayTime = settings.OverlayTimeInt;
            bool enableOverlay = settings.EnableOverlay == true;
            var positionSet = settings.ViewPosition[settings.ViewPosition.Keys.ElementAt(settings.PositionIndex)];
            HorizontalAlignment overlayHorizontalAlignment = positionSet.HorizontalAlignment;
            VerticalAlignment overlayVerticalAlignment = positionSet.VerticalAlignment;
            bool aero = settings.EnableAero == true;
            double imageGridWidth = settings.OverlayX;
            double imageGridHeight = settings.OverlayY;
            bool enableCursor = settings.EnableCursor == true;
            int captureMode = settings.CaptureModeIndex;
            bool enableSetArrow = settings.EnableSetArrow == true;
            System.Drawing.Imaging.PixelFormat pixelFormat = settings.PixelFormats.Keys.ElementAt(settings.PixelFormatIndex);
            int compressMode = (int)settings.CompressSelect;
            string compressOption = "";
            if (settings.CompressSelect == CompressType.Optipng)
            {
                compressOption = settings.CompressNums.Keys.ElementAt(settings.CompressIndex);
            }
            else if (settings.CompressSelect == CompressType.Zopfli)
            {
                compressOption = settings.CompressNumsZopfli.Keys.ElementAt(settings.CompressIndexZopfli);
            }
            bool enabledOvarlayTabName = settings.OverlayTabNameEnabled == true;
            int tabNumber = settings.TabNumber;
            bool enabledOverlayFileName = settings.OverlayFileNameEnabled == true;
            try
            {
                IntPtr windowHandle = GetForegroundWindow();

                // ★ キャプチャ時にファイル名を設定：Windows標準ファイル保存ダイアログを表示
                if (settings.EnableSetFileNameOnCapture == true)
                {
                    string currentExt = imageFormatName.ToLower(); // "png" or "jpg"

                    using (var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonSaveFileDialog())
                    {
                        dialog.Title = "保存ファイルを指定";
                        // 初期ディレクトリ：現在のDirectory設定を使用
                        if (Directory.Exists(dirName))
                        {
                            dialog.InitialDirectory = dirName;
                        }
                        // 初期ファイル名：現在のFileName設定を使用
                        dialog.DefaultFileName = fileName;
                        dialog.DefaultExtension = currentExt;
                        dialog.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter(currentExt.ToUpper() + " ファイル", "*." + currentExt));
                        dialog.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("すべてのファイル", "*.*"));
                        dialog.AlwaysAppendDefaultExtension = true;
                        dialog.EnsureValidNames = true;

                        // キャプチャツール本体が前面に出ないよう、非表示のダミーウィンドウをオーナーにする
                        var dummyOwner = new Window
                        {
                            Width = 0,
                            Height = 0,
                            WindowStyle = WindowStyle.None,
                            ShowInTaskbar = false,
                            Opacity = 0,
                            Left = -10000,
                            Top = -10000,
                            AllowsTransparency = true,
                        };
                        dummyOwner.Show();
                        try
                        {
                            if (dialog.ShowDialog(dummyOwner) != Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
                            {
                                // キャンセル時はキャプチャしない
                                return false;
                            }

                            string selectedFullPath = dialog.FileName;
                            string selectedDir = Path.GetDirectoryName(selectedFullPath);
                            string selectedFileNameWithoutExt = Path.GetFileNameWithoutExtension(selectedFullPath);

                            // 選択結果を settings に反映（Directory・FileName 両方を更新）
                            settings.Directory = selectedDir;
                            settings.FileName = selectedFileNameWithoutExt;

                            // このキャプチャで使うローカル変数も更新
                            dirName = selectedDir;
                            fileName = selectedFileNameWithoutExt;
                        }
                        finally
                        {
                            // 保存・キャンセルどちらのルートでも確実にクローズ
                            dummyOwner.Close();
                        }
                    }
                }

                var convResult = doNameRegexConverts(fileName, dirName, screenFlag, windowHandle, settings);
                fileName = convResult.Item1;
                dirName = convResult.Item2;

                if (settings.EnableAutoContinueCount == true)
                {
                    settings.NumberCount = GetContinueFileName(settings, dirName, fileName);
                }

                CreateDirectory(dirName);
                string fullPath = dirName + "\\" + settings.GetSampleFileName(settings.NumberCount, fileName);

                if (File.Exists(fullPath))
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullPath);
                    string extension = Path.GetExtension(fullPath);
                    fullPath = dirName + "\\" + fileNameWithoutExtension + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                }

                //オーバーレイ非表示
                if (prevOverlayWindow != null && prevOverlayWindow.ClosingReady)
                {
                    prevOverlayWindow.ImageSource = null;
                    prevOverlayWindow.gridView.Opacity = 0;
                    prevOverlayWindow.Close();
                    prevOverlayWindow = null;
                }

                using (Bitmap bitmap = CaptureControl(windowHandle, captureMode, false, screenFlag, aero, enableCursor, enableSetArrow, pixelFormat))
                {
                    ImageFormat imageFormat;
                    if (imageFormatName.ToUpper().CompareTo("PNG") == 0)
                    {
                        imageFormat = ImageFormat.Png;
                    }
                    else if (imageFormatName.ToUpper().CompareTo("JPG") == 0)
                    {
                        imageFormat = ImageFormat.Jpeg;
                    }
                    else
                    {
                        imageFormat = ImageFormat.Png;
                    }

                    if (imageFormat == ImageFormat.Png)
                    {
                        if (compressMode == 1)
                        {
                            bitmap.Save(fullPath, imageFormat);

                            Task.Run(() =>
                            {
                                ProcessStartInfo compressStartInfo = new ProcessStartInfo(callCompressPath, string.Format("\"{0}\" \"{1}\" {2}", optipngPath, fullPath, compressOption)) { WindowStyle = ProcessWindowStyle.Hidden };
                                Process.Start(compressStartInfo);
                            });

                            //Task.Run(() =>
                            //{
                            //    ProcessStartInfo compressStartInfo = new ProcessStartInfo(AppDomain.CurrentDomain.BaseDirectory + "optipng.exe", fullPath + " " + compressOption) { WindowStyle = ProcessWindowStyle.Hidden };
                            //    Process.Start(compressStartInfo);
                            //});
                        }
                        else if (compressMode == 2)
                        {
                            string nowHMS = DateTime.Now.ToString("HHmmss");
                            string tmpName = tempPath + "CaptureToolTmpFile_" + nowHMS + ".png";
                            bitmap.Save(tmpName, imageFormat);
                            File.Create(fullPath).Dispose();

                            Task.Run(() =>
                            {
                                ProcessStartInfo compressStartInfo = new ProcessStartInfo(callCompressPath, string.Format("\"{0}\" {1} \"{2}\" \"{3}\"", zopflipngPath, compressOption, tmpName, fullPath)) { WindowStyle = ProcessWindowStyle.Hidden };
                                Process.Start(compressStartInfo);
                            });

                            //Task.Run(() =>
                            //{
                            //    ProcessStartInfo compressStartInfo = new ProcessStartInfo(AppDomain.CurrentDomain.BaseDirectory + "zopflipng.exe", compressOption + " " + tmpName + " " + fullPath) { WindowStyle = ProcessWindowStyle.Hidden };
                            //    Process.Start(compressStartInfo);
                            //});
                        }
                        else
                        {
                            bitmap.Save(fullPath, imageFormat);
                        }
                    }
                    else
                    {
                        bitmap.Save(fullPath, imageFormat);
                    }

                    ImageSource imageSource = Extend.ConvertBitmapToBitmapImage(bitmap);
                    imageSource.Freeze();
                    if (enableOverlay)
                    {
                        StringBuilder overlayText = new StringBuilder();
                        if (enabledOvarlayTabName)
                        {
                            overlayText.Append((tabNumber + 1).ToString());
                            overlayText.Append(": ");
                        }
                        if (enabledOverlayFileName)
                        {
                            string overlayFilename = "";
                            try
                            {
                                overlayFilename = Path.GetFileName(fullPath);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                            overlayText.Append(overlayFilename);
                            overlayText.Append(": ");
                        }
                        OverlayWindowDataContext overlayWindowDataContext = new OverlayWindowDataContext() { OverlayText = overlayText.ToString() };
                        OverlayWindow overlayWindow = new OverlayWindow()
                        {
                            ImageSource = imageSource,
                            OverlayTime = overlayTime,
                            OverlayHorizontalAlignment = overlayHorizontalAlignment,
                            OverlayVerticalAlignment = overlayVerticalAlignment,
                            ImageGridWidth = imageGridWidth,
                            ImageGridHeight = imageGridHeight,
                            DataContext = overlayWindowDataContext
                        };
                        overlayWindow.Show();
                        prevOverlayWindow = overlayWindow;
                    }

                }
            }
            catch (Exception ex)
            {
                OverlayDialog overlayDialog = new OverlayDialog(ex.Message)
                {
                    Owner = MainWindow.GetMainWindow()
                };
                overlayDialog.Show();
                MainWindow.logger.Error(ex.Message);
                Debug.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public static ImageSource GetImageSourceFromWindow(IntPtr handle, int mode, bool enableCursor, bool enableSetArrow, System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            Bitmap bitmap = CaptureControl(handle, mode, true, false, true, enableCursor, enableSetArrow, pixelFormat);
            return Extend.ConvertBitmapToBitmapImage(bitmap);
        }

        private static DesktopDuplication.DesktopDuplicator desktopDuplicator;
        private static int DesktopDuplicatorNum = -1;
        private static List<System.Windows.Forms.Screen> AllScreens;
        private static Bitmap CaptureControl(IntPtr handle, int mode, bool extend, bool screenFlag, bool aero, bool enableCursor, bool enableSetArrow, System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            int width;
            int height;
            RECT rect;
            var activeDisplay = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
            int displayIndex = AllScreens.IndexOf(activeDisplay);

            if (screenFlag)
            {
                if (mode == 0)
                {
                    GetWindowRect(handle, out rect);
                    width = rect.right - rect.left;
                    height = rect.bottom - rect.top;
                    if (width <= 0)
                    {
                        width = activeDisplay.Bounds.Width;
                    }
                    if (height <= 0)
                    {
                        height = activeDisplay.Bounds.Height;
                    }
                }
                else
                {
                    width = activeDisplay.Bounds.Width;
                    height = activeDisplay.Bounds.Height;
                    rect = new RECT() { left = activeDisplay.Bounds.Left, right = width, top = activeDisplay.Bounds.Top, bottom = height };
                }
            }
            else
            {
                if (mode == 0)
                {
                    aero = false;
                }
                int rectSize;
                unsafe
                {
                    rectSize = sizeof(RECT);
                }
                if (aero)
                {
                    DwmGetWindowAttribute(handle, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out rect, rectSize);
                }
                else
                {
                    GetWindowRect(handle, out rect);
                }
                if (extend)
                {
                    rect.left = rect.left - 1;
                    rect.right = rect.right + 1;
                    rect.top = rect.top - 1;
                    rect.bottom = rect.bottom + 1;
                }
                width = rect.right - rect.left;
                height = rect.bottom - rect.top;
                if (width <= 0)
                {
                    width = activeDisplay.Bounds.Width;
                }
                if (height <= 0)
                {
                    height = activeDisplay.Bounds.Height;
                }
            }

            //BitmapからGraphicsを作成
            Bitmap img = new Bitmap(width, height);
            //Bitmap img = new Bitmap(width, height, pixelFormat);
            Graphics memg = Graphics.FromImage(img);
            if (mode == 0)
            {
                IntPtr dc = memg.GetHdc();
                PrintWindow(handle, dc, 0);
                memg.ReleaseHdc(dc);
                if (screenFlag)
                {
                    int pwidth = activeDisplay.Bounds.Width;
                    int pheight = activeDisplay.Bounds.Height;
                    System.Drawing.Rectangle rectangle = System.Drawing.Rectangle.FromLTRB(0, 0, pwidth, pheight);
                    Bitmap img2 = new Bitmap(pwidth, pheight);
                    //Bitmap img = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    Graphics memg2 = Graphics.FromImage(img2);
                    memg.Dispose();
                    memg2.CopyFromScreen(activeDisplay.Bounds.Left, activeDisplay.Bounds.Top, 0, 0, rectangle.Size, CopyPixelOperation.SourceCopy);
                    memg2.DrawImage(img, new PointF(rect.left - activeDisplay.Bounds.Left, rect.top - activeDisplay.Bounds.Top));
                    memg = memg2;
                    img = img2;
                }
            }
            else if (mode == 1)
            {
                System.Drawing.Rectangle rectangle = System.Drawing.Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
                memg.CopyFromScreen(rect.left, rect.top, 0, 0, rectangle.Size, CopyPixelOperation.SourceCopy);
            }
            else if (mode == 3)
            {
                if (DesktopDuplicatorNum != displayIndex)
                {
                    if (desktopDuplicator != null)
                    {
                        desktopDuplicator.Dispose();
                        desktopDuplicator = null;
                    }
                    desktopDuplicator = new DesktopDuplication.DesktopDuplicator(displayIndex);
                    DesktopDuplicatorNum = displayIndex;
                    desktopDuplicator.GetLatestFrameWithCheck();
                }
                DesktopDuplication.DesktopFrame frame = null;
                int tryCount = 0;
                StringBuilder message = new StringBuilder();
                while (tryCount < 3)
                {
                    try
                    {
                        frame = desktopDuplicator.GetLatestFrameWithCheck();
                        if (frame == null)
                        {
                            message.Append(tryCount + ": " + "frame == null" + "\n");
                            MainWindow.logger.Error("frame == null");
                            if (tryCount >= 2)
                            {
                                throw new Exception(message.ToString());
                            }
                            tryCount++;
                            System.Threading.Thread.Sleep(3000);
                            continue;
                        }
                        Bitmap dupBitmap = frame.DesktopImage.Clone(new Rectangle(rect.left - activeDisplay.Bounds.Left, rect.top - activeDisplay.Bounds.Top, width, height), pixelFormat);
                        //Bitmap dupBitmap = frame.DesktopImage;
                        //dupBitmap.Save($@"C:\Users\zz030209\Pictures\Capture\Test\dupTest{desktopDuplicator1.GetHashCode()}.png");
                        memg.DrawImage(dupBitmap, 0, 0);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(tryCount + ": " + ex.Message);
                        message.Append(tryCount + ": " + ex.Message + "\n");
                        MainWindow.logger.Error(ex.Message + Environment.NewLine + ex.StackTrace);
                        if (desktopDuplicator.Disposed == false)
                            desktopDuplicator.Dispose();
                        desktopDuplicator = new DesktopDuplication.DesktopDuplicator(displayIndex);
                        if (tryCount >= 2)
                        {
                            throw new Exception(message.ToString());
                        }
                        tryCount++;
                        //if (ex.Message.Equals("Failed to acquire next frame."))
                        //{
                        //    System.Threading.Thread.Sleep(200);
                        //    Debug.WriteLine("200ms wait");
                        //}
                        System.Threading.Thread.Sleep(3000);
                    }
                }
            }
            else
            {
                RECT rect2 = new RECT();
                IntPtr targetDC;
                if (screenFlag)
                {
                    IntPtr disDC = GetDC(IntPtr.Zero);
                    targetDC = disDC;
                }
                else
                {
                    GetWindowRect(handle, out rect2);
                    IntPtr winDC = GetWindowDC(handle);
                    targetDC = winDC;
                }
                IntPtr hDC = memg.GetHdc();
                BitBlt(hDC, 0, 0, img.Width, img.Height, targetDC, rect.left - rect2.left, rect.top - rect2.top, SRCCOPY);
                memg.ReleaseHdc();
            }


            //マウスカーソル関連
            if (enableCursor)
            {
                if (enableSetArrow)
                {
                    WriteCursorToGrap(memg, rect.left, rect.top, enableSetArrow);
                }
                else
                {
                    WriteCursorToGrap2(memg, rect.left, rect.top, enableSetArrow);
                }
            }
            //GraphicsをDisposeでBitmapに画像が入る
            memg.Dispose();

            if (pixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                Bitmap encoded = img.Clone(new Rectangle(0, 0, img.Width, img.Height), pixelFormat);
                return encoded;
            }
            else
            {
                return img;
            }
        }

        public static void WriteCursorToGrap(Graphics g, int left, int top, bool enableSetArrow)
        {
            System.Windows.Forms.Cursor cursor;
            if (enableSetArrow)
            {
                cursor = System.Windows.Forms.Cursors.Arrow;
            }
            else
            {
                cursor = new System.Windows.Forms.Cursor(System.Windows.Forms.Cursor.Current.Handle);
            }
            System.Drawing.Point cPoint = System.Windows.Forms.Cursor.Position;
            System.Drawing.Point hSpot = cursor.HotSpot;
            System.Drawing.Point DrPosition = new System.Drawing.Point((cPoint.X - hSpot.X - left), (cPoint.Y - hSpot.Y - top));
            cursor.Draw(g, new Rectangle(DrPosition, cursor.Size));
        }


        public static void WriteCursorToGrap2(Graphics g, int left, int top, bool enableSetArrow)
        {
            var cInfo = new CURSORINFO
            {
                cbSize = Marshal.SizeOf(typeof(CURSORINFO))
            };
            if (!GetCursorInfo(out cInfo) || cInfo.flags != CURSOR_SHOWING)
            {
                return;
            }
            IntPtr cIcon = CopyIcon(cInfo.hCursor);
            if (cIcon == IntPtr.Zero)
            {
                return;
            }

            var oIcon = System.Drawing.Icon.FromHandle(cIcon);
            var oBitmap = oIcon.ToBitmap();
            if (!GetIconInfo(cIcon, out ICONINFO iconInfo))
            {
                return;
            }
            System.Drawing.Point DrPosition = new System.Drawing.Point((cInfo.ptScreenPos.X - iconInfo.xHotspot - left), (cInfo.ptScreenPos.Y - iconInfo.yHotspot - top));
            Bitmap hbmMask;
            Bitmap hbmColor;
            if (iconInfo.hbmMask != IntPtr.Zero)
            {
                hbmMask = System.Drawing.Image.FromHbitmap(iconInfo.hbmMask);
                Bitmap tempB = new Bitmap(hbmMask.Width, hbmMask.Width);
                Bitmap topMask = null;
                Bitmap underMask = null;
                if (hbmMask.Height == hbmMask.Width * 2 && oBitmap.Width == hbmMask.Width && oBitmap.Height == hbmMask.Width)
                {
                    underMask = hbmMask.Clone(new Rectangle(0, hbmMask.Width, hbmMask.Width, hbmMask.Width), hbmMask.PixelFormat);
                }
                else if (iconInfo.hbmColor != IntPtr.Zero)
                {
                    //topMask = oBitmap.Clone(new Rectangle(0, 0, oBitmap.Width, oBitmap.Height), oBitmap.PixelFormat);
                    topMask = hbmMask.Clone(new Rectangle(0, 0, hbmMask.Width, hbmMask.Width), hbmMask.PixelFormat);
                    hbmColor = System.Drawing.Image.FromHbitmap(iconInfo.hbmColor);
                    underMask = hbmColor;
                }
                else
                {
                    g.DrawImage(oBitmap, DrPosition.X, DrPosition.Y);
                    return;
                }
                for (int tx = 0; tx < hbmMask.Width; tx++)
                {
                    for (int ty = 0; ty < hbmMask.Width; ty++)
                    {
                        System.Drawing.Color oc = oBitmap.GetPixel(tx, ty);
                        System.Drawing.Color tc;
                        if (topMask == null)
                        {
                            tc = oc;
                        }
                        else
                        {
                            tc = topMask.GetPixel(tx, ty);
                            //tc = System.Drawing.Color.FromArgb(oc.A, tc.R ^ oc.R, tc.G ^ oc.G, tc.B ^ oc.B);
                        }
                        System.Drawing.Color uc = underMask.GetPixel(tx, ty);
                        System.Drawing.Color xorColor = System.Drawing.Color.FromArgb(oc.A, tc.R ^ uc.R, tc.G ^ uc.G, tc.B ^ uc.B);
                        //if (topMask != null && tc.ToArgb() == System.Drawing.Color.White.ToArgb())
                        //{
                        //    xorColor = System.Drawing.Color.Transparent;
                        //}
                        tempB.SetPixel(tx, ty, xorColor);
                    }
                }
                g.DrawImage(tempB, DrPosition.X, DrPosition.Y);
                return;
            }
            g.DrawIcon(oIcon, DrPosition.X, DrPosition.Y);
        }

        public static System.Windows.Media.Imaging.BitmapSource GetBitmapSourceFromIconHandle(IntPtr hIcon)
        {
            System.Windows.Media.Imaging.BitmapSource bitmapSource = null;
            try
            {
                bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return bitmapSource;
        }

        public static Bitmap ConvertBitmapSourceToBitmap(System.Windows.Media.Imaging.BitmapSource bitmapSource)
        {
            var bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bitmapSource.CopyPixels(System.Windows.Int32Rect.Empty, bitmapData.Scan0, bitmapData.Height * bitmapData.Stride, bitmapData.Stride);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static MatchCollection FileNameNumberSearch(string fileName)
        {
            Regex regex = new Regex("[0-9]+");
            return regex.Matches(fileName);
        }

        public static void CreateFileNameNumberCountButtons(string fileName, StackPanel owner, Settings settings, int mode)
        {
            MatchCollection matchCollection = FileNameNumberSearch(fileName);
            if (matchCollection.Count > 0)
            {
                owner.Height = 22;
            }
            else
            {
                owner.Height = 0;
            }
            Grid[] grids = new Grid[matchCollection.Count];
            int count = 0;
            owner.Children.Clear();
            foreach (Match match in matchCollection)
            {
                Match tmpMatch = match;
                TextBlock textBlock = new TextBlock() { Text = match.Value + ":", Margin = new Thickness(0, 0, 0.5, 0), Focusable = false };
                StackPanel stackPanel = new StackPanel() { Margin = new Thickness(0, 0, 3, 0), Focusable = false };
                Button upButton = new Button() { Content = new TextBlock() { Text = "▲", RenderTransformOrigin = new System.Windows.Point(0.5, 0.5), RenderTransform = new ScaleTransform(3.5, 1.3) }, FontSize = 5, MinWidth = 20, Focusable = false };
                Button downButton = new Button() { Content = new TextBlock() { Text = "▼", RenderTransformOrigin = new System.Windows.Point(0.5, 1), RenderTransform = new ScaleTransform(3.5, 1.3) }, FontSize = 5, MinWidth = 20, Focusable = false };
                if (mode == 0)
                {
                    upButton.Click += (sender, e) =>
                    {
                        if (int.TryParse(match.Value, out int parsed))
                        {
                            string numberFormat = "{0:D" + tmpMatch.Length + "}";
                            string formatedNumber = string.Format(numberFormat, parsed + 1);
                            settings.FileName = settings.FileName.ReplaceAt(tmpMatch.Index, tmpMatch.Length, formatedNumber);
                            textBlock.Text = formatedNumber.ToString() + ":";
                        }
                    };
                    downButton.Click += (sender, e) =>
                    {
                        if (int.TryParse(match.Value, out int parsed))
                        {
                            string numberFormat = "{0:D" + tmpMatch.Length + "}";
                            string formatedNumber = string.Format(numberFormat, parsed - 1);
                            settings.FileName = settings.FileName.ReplaceAt(tmpMatch.Index, tmpMatch.Length, formatedNumber);
                            textBlock.Text = formatedNumber.ToString() + ":";
                        }
                    };
                }
                else
                {
                    upButton.Click += (sender, e) =>
                    {
                        if (int.TryParse(match.Value, out int parsed))
                        {
                            string numberFormat = "{0:D" + tmpMatch.Length + "}";
                            string formatedNumber = string.Format(numberFormat, parsed + 1);
                            settings.Directory = settings.Directory.ReplaceAt(tmpMatch.Index, tmpMatch.Length, formatedNumber);
                        }
                    };
                    downButton.Click += (sender, e) =>
                    {
                        if (int.TryParse(match.Value, out int parsed))
                        {
                            string numberFormat = "{0:D" + tmpMatch.Length + "}";
                            string formatedNumber = string.Format(numberFormat, parsed - 1);
                            settings.Directory = settings.Directory.ReplaceAt(tmpMatch.Index, tmpMatch.Length, formatedNumber);
                        }
                    };
                }
                stackPanel.Children.Add(upButton);
                stackPanel.Children.Add(downButton);
                owner.Children.Add(textBlock);
                owner.Children.Add(stackPanel);
                count++;
            }
        }

        public static void CreateFileNameNumberCountButtons(string fileName, StackPanel owner, Settings settings)
        {
            CreateFileNameNumberCountButtons(fileName, owner, settings, 0);
        }

        public static void CreateFolderNameNumberCountButtons(string fileName, StackPanel owner, Settings settings)
        {
            CreateFileNameNumberCountButtons(fileName, owner, settings, 1);
        }

        public static string ReplaceAt(this string str, int index, int length, string replace)
        {
            return str.Remove(index, Math.Min(length, str.Length - index)).Insert(index, replace);
        }

        public static string GetClipBoardText()
        {
            string result = null;
            while (result == null)
            {
                try
                {
                    result = Clipboard.GetText(TextDataFormat.Text);
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return result;
        }

        public static int GetContinueFileNameOld(Settings settings)
        {
            string numberFormat = "{0:D" + settings.NumberDigits + "}";
            for (int index = 0; ; index++)
            {
                string tmpSampleFileName = settings.GetSampleFileName(index);
                if (!File.Exists(settings.Directory + "\\" + tmpSampleFileName))
                {
                    return index;
                }
            }
        }

        public static int GetContinueFileNameNoRegex(Settings settings)
        {
            return GetContinueFileName(settings, settings.Directory, settings.FileName);
        }

        public static int GetContinueFileName(Settings settings)
        {
            var convResult = doNameRegexConverts(settings.FileName, settings.Directory, false, IntPtr.Zero, settings);
            return GetContinueFileName(settings, convResult.Item2, convResult.Item1);
        }


        public static int GetContinueFileName(Settings settings, string dirName, string fileName)
        {
            try
            {
                if (!System.IO.Directory.Exists(dirName))
                {
                    return 0;
                }
                var fList = System.IO.Directory.EnumerateFiles(dirName).Select(path => Path.GetFileName(path))
                    .Where(str => { if (Regex.Match(str, fileName + settings.CountConju + "[0-9]{" + settings.NumberDigits + "}\\." + settings.SaveFormats[(SaveFormat)settings.SaveFormatIndex]).Success) { return true; } return false; })
                    .OrderByDescending(x => x);
                if (fList.Count() > 0)
                {
                    var lastName = fList.ElementAt(0);
                    var countStr = Path.GetFileNameWithoutExtension(lastName).Replace(fileName + settings.CountConju, "");
                    if (int.TryParse(countStr, out int result))
                    {
                        return result + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                OverlayDialog overlayDialog = new OverlayDialog(ex.Message)
                {
                    Owner = MainWindow.GetMainWindow()
                };
                overlayDialog.Show();
                MainWindow.logger.Error(ex.Message);
                Debug.WriteLine(ex.Message);
            }
            return settings.NumberCount;
        }
    }
}
