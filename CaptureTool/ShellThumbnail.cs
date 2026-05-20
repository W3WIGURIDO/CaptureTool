using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CaptureTool
{
    internal static class ShellThumbnail
    {
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("BCC18B79-BA16-442F-80C4-8A59C30C463B")]
        private interface IShellItemImageFactory
        {
            [PreserveSig]
            int GetImage(SIZE size, uint flags, out IntPtr phbm);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE { public int cx, cy; }

        // SIIGBF_RESIZETOFIT = 0x0
        private const uint SIIGBF_RESIZETOFIT = 0x0;

        private static readonly Guid IID_IShellItemImageFactory
            = new Guid("BCC18B79-BA16-442F-80C4-8A59C30C463B");

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SHCreateItemFromParsingName(
            string pszPath, IntPtr pbc, ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory ppv);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        // Shell取得を試み、失敗時はファイルから直接ロードする
        public static BitmapSource GetThumbnail(string filePath, int width, int height)
        {
            return TryGetShellThumbnail(filePath, width, height)
                ?? TryLoadFromFile(filePath, width);
        }

        private static BitmapSource TryGetShellThumbnail(string filePath, int width, int height)
        {
            IShellItemImageFactory factory = null;
            try
            {
                Guid iid = IID_IShellItemImageFactory;
                SHCreateItemFromParsingName(filePath, IntPtr.Zero, ref iid, out factory);
                if (factory == null) return null;

                int hr = factory.GetImage(
                    new SIZE { cx = width, cy = height }, SIIGBF_RESIZETOFIT, out IntPtr hbm);
                if (hr != 0 || hbm == IntPtr.Zero) return null;

                try
                {
                    var src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hbm, IntPtr.Zero, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    src.Freeze();
                    return src;
                }
                finally
                {
                    DeleteObject(hbm);
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (factory != null)
                    Marshal.ReleaseComObject(factory);
            }
        }

        private static BitmapSource TryLoadFromFile(string filePath, int decodeWidth)
        {
            try
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(filePath);
                bi.DecodePixelWidth = decodeWidth;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bi.EndInit();
                bi.Freeze();
                return bi;
            }
            catch
            {
                return null;
            }
        }
    }
}