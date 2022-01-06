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

        static MainProcess()
        {
            AllScreens = System.Windows.Forms.Screen.AllScreens.ToList();
            AllScreens.Remove(System.Windows.Forms.Screen.PrimaryScreen);
            AllScreens.Insert(0, System.Windows.Forms.Screen.PrimaryScreen);
        }

        public static OverlayWindow prevOverlayWindow;
        private const string FormatText = "Text";

        public static bool CaptureScreen(string fileName, string dirName, string imageFormatName = "Png", int overlayTime = 3000, bool enableOverlay = true, HorizontalAlignment overlayHorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment overlayVerticalAlignment = VerticalAlignment.Top, bool screenFlag = true, bool aero = true, double imageGridWidth = 200, double imageGridHeight = 150, bool enableCursor = false, int captureMode = 0, bool enableSetArrow = false, System.Drawing.Imaging.PixelFormat pixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb)
        {
            const string WordWindowTitle = "<WindowTitle>";
            const string WordDate = "<Date>";
            const string WordTime = "<Time>";
            const string DesktopText = "Desktop";
            try
            {
                IntPtr windowHandle = GetForegroundWindow();
                Process process = null;
                void GetProcessFromHandle()
                {
                    if (process == null)
                    {
                        try
                        {
                            GetWindowThreadProcessId(windowHandle, out int processID);
                            process = Process.GetProcessById(processID);
                        }
                        catch (Exception pex)
                        {
                            Console.WriteLine(pex.Message);
                        }
                    }
                }

                if (fileName.Contains(WordWindowTitle))
                {
                    if (screenFlag)
                    {
                        fileName = fileName.Replace(WordWindowTitle, DesktopText);
                    }
                    else
                    {
                        GetProcessFromHandle();
                        string title = process.MainWindowTitle;
                        if (string.IsNullOrEmpty(title))
                        {
                            title = DesktopText;
                        }
                        title = System.Text.RegularExpressions.Regex.Replace(title, "[\\\\/:*?\"<>|]", string.Empty);
                        fileName = fileName.Replace(WordWindowTitle, title);
                        dirName = dirName.Replace(WordWindowTitle, title);
                    }
                }
                if (fileName.Contains(WordDate))
                {
                    string nowYMD = DateTime.Now.ToString("yyyyMMdd");
                    fileName = fileName.Replace(WordDate, nowYMD);
                    dirName = dirName.Replace(WordDate, nowYMD);
                }
                if (fileName.Contains(WordTime))
                {
                    string nowHMS = DateTime.Now.ToString("HHmmss");
                    fileName = fileName.Replace(WordTime, nowHMS);
                    dirName = dirName.Replace(WordTime, nowHMS);
                }
                CreateDirectory(dirName);

                if (prevOverlayWindow != null && prevOverlayWindow.ClosingReady)
                {
                    prevOverlayWindow.Close();
                }

                Bitmap bitmap = CaptureControl(windowHandle, captureMode, false, screenFlag, aero, enableCursor, enableSetArrow, pixelFormat);
                //ImageCodecInfo codecInfo = null;
                //foreach(ImageCodecInfo tmpInfo in ImageCodecInfo.GetImageEncoders())
                //{
                //    if (tmpInfo.FormatID == ImageFormat.Png.Guid)
                //    {

                //    }
                //}
                if (File.Exists(fileName))
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);
                    fileName = dirName + "\\" + fileNameWithoutExtension + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                }
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
                bitmap.Save(fileName, imageFormat);
                ImageSource imageSource = Extend.ConvertBitmapToBitmapImage(bitmap);
                if (enableOverlay)
                {
                    OverlayWindow overlayWindow = new OverlayWindow()
                    {
                        ImageSource = imageSource,
                        OverlayTime = overlayTime,
                        OverlayHorizontalAlignment = overlayHorizontalAlignment,
                        OverlayVerticalAlignment = overlayVerticalAlignment,
                        ImageGridWidth = imageGridWidth,
                        ImageGridHeight = imageGridHeight
                    };
                    overlayWindow.Show();
                    prevOverlayWindow = overlayWindow;
                }
            }
            catch (Exception ex)
            {
                WpfFolderBrowser.CustomMessageBox.Show(MainWindow.ActiveWindow, ex.Message, "例外", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None);
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
                    desktopDuplicator = new DesktopDuplication.DesktopDuplicator(displayIndex);
                    DesktopDuplicatorNum = displayIndex;
                    desktopDuplicator.GetLatestFrame();
                }
                DesktopDuplication.DesktopFrame frame = null;
                int tryCount = 0;
                while (tryCount < 3)
                {
                    try
                    {
                        frame = desktopDuplicator.GetLatestFrame();
                        if (frame == null)
                        {
                            tryCount++;
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
                        Debug.WriteLine(ex.Message);
                        desktopDuplicator = new DesktopDuplication.DesktopDuplicator(displayIndex);
                        if (tryCount > 1)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        tryCount++;
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

        public static void CreateFileNameNumberCountButtons(string fileName, StackPanel owner, Settings settings)
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
                TextBlock textBlock = new TextBlock() { Text = count + ":" };
                StackPanel stackPanel = new StackPanel() { Margin = new Thickness(0, 0, 2, 0) };
                Button upButton = new Button() { Content = new TextBlock() { Text = "▲", RenderTransformOrigin = new System.Windows.Point(0.5, 0.5), RenderTransform = new ScaleTransform(3.5, 1.3) }, FontSize = 5, MinWidth = 20 };
                upButton.Click += (sender, e) =>
                {
                    if (int.TryParse(match.Value, out int parsed))
                    {
                        string numberFormat = "{0:D" + tmpMatch.Length + "}";
                        string formatedNumber = string.Format(numberFormat, parsed + 1);
                        settings.FileName = settings.FileName.ReplaceAt(tmpMatch.Index, tmpMatch.Length, formatedNumber);
                    }
                };
                Button downButton = new Button() { Content = new TextBlock() { Text = "▼", RenderTransformOrigin = new System.Windows.Point(0.5, 1), RenderTransform = new ScaleTransform(3.5, 1.3) }, FontSize = 5, MinWidth = 20 };
                downButton.Click += (sender, e) =>
                {
                    if (int.TryParse(match.Value, out int parsed))
                    {
                        string numberFormat = "{0:D" + tmpMatch.Length + "}";
                        string formatedNumber = string.Format(numberFormat, parsed - 1);
                        settings.FileName = settings.FileName.ReplaceAt(tmpMatch.Index, tmpMatch.Length, formatedNumber);
                    }
                };
                stackPanel.Children.Add(upButton);
                stackPanel.Children.Add(downButton);
                owner.Children.Add(textBlock);
                owner.Children.Add(stackPanel);
                count++;
            }
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

        public static int GetContinueFileName(Settings settings)
        {
            string numberFormat = "{0:D" + settings.NumberDigits + "}";
            for (int index = 0; ; index++)
            {
                string formatedNumber = string.Format(numberFormat, index);
                string tmpSampleFileName = settings.FileName + formatedNumber + "." + settings.SaveFormats[(SaveFormat)settings.SaveFormatIndex];
                if (!File.Exists(settings.Directory + "\\" + tmpSampleFileName))
                {
                    return index;
                }
            }
        }
    }
}
