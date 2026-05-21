using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading;

namespace CaptureTool
{
    internal static class DesktopDuplicationCapture
    {
        private static DesktopDuplication.DesktopDuplicator _duplicator;
        private static int _duplicatorNum = -1;

        public static void Capture(
            MainProcess.RECT rect,
            int displayIndex,
            System.Windows.Forms.Screen activeDisplay,
            PixelFormat pixelFormat,
            Graphics memg,
            int width,
            int height)
        {
            if (_duplicatorNum != displayIndex)
            {
                if (_duplicator != null)
                {
                    _duplicator.Dispose();
                    _duplicator = null;
                }
                _duplicator = new DesktopDuplication.DesktopDuplicator(displayIndex);
                _duplicatorNum = displayIndex;
                _duplicator.GetLatestFrameWithCheck();
            }

            DesktopDuplication.DesktopFrame frame = null;
            int tryCount = 0;
            StringBuilder message = new StringBuilder();
            while (tryCount < 3)
            {
                try
                {
                    frame = _duplicator.GetLatestFrameWithCheck();
                    if (frame == null)
                    {
                        message.Append(tryCount + ": frame == null\n");
                        MainWindow.logger.Error("frame == null");
                        if (tryCount >= 2)
                        {
                            throw new Exception(message.ToString());
                        }
                        tryCount++;
                        Thread.Sleep(3000);
                        continue;
                    }
                    Bitmap dupBitmap = frame.DesktopImage.Clone(
                        new Rectangle(
                            rect.left - activeDisplay.Bounds.Left,
                            rect.top - activeDisplay.Bounds.Top,
                            width,
                            height),
                        pixelFormat);
                    memg.DrawImage(dupBitmap, 0, 0);
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(tryCount + ": " + ex.Message);
                    message.Append(tryCount + ": " + ex.Message + "\n");
                    MainWindow.logger.Error(ex.Message + Environment.NewLine + ex.StackTrace);
                    if (_duplicator.Disposed == false)
                        _duplicator.Dispose();
                    _duplicator = new DesktopDuplication.DesktopDuplicator(displayIndex);
                    if (tryCount >= 2)
                    {
                        throw new Exception(message.ToString());
                    }
                    tryCount++;
                    Thread.Sleep(3000);
                }
            }
        }
    }
}