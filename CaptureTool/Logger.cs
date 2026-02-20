using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CaptureTool
{
    public class Logger : IDisposable
    {
        private static readonly Encoding enc = Encoding.GetEncoding("Shift_JIS");

        private string path;
        private Level level;
        private System.IO.StreamWriter sw = null;
        public Logger(string path, int level)
        {
            if (level < 0)
                level = 0;
            if (level > 3)
                level = 3;
            this.path = path;
            this.level = (Level)Enum.ToObject(typeof(Level), level);
            sw = new System.IO.StreamWriter(path, true, enc);
        }

        private void WriteLog(string text, Level level)
        {
            try
            {
                sw?.WriteLine(string.Format("[{0}]<{1}>: {2}", level.ToString(), DateTime.Now, text));
            }
            catch (Exception ex)
            {
                WpfFolderBrowser.CustomMessageBox.Show(MainWindow.ActiveWindow, ex.Message, "例外", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None);
            }
        }

        public void Error(string text)
        {
            if (Level.Error <= level)
            {
                WriteLog(text, Level.Error);
            }
        }

        public void Info(string text)
        {
            if (Level.Info <= level)
            {
                WriteLog(text, Level.Info);
            }
        }

        public void Debug(string text)
        {
            if (Level.Debug <= level)
            {
                WriteLog(text, Level.Debug);
            }
        }

        public void Trace(string text)
        {
            if (Level.Trace <= level)
            {
                WriteLog(text, Level.Trace);
            }
        }

        public void Dispose()
        {
            sw?.Close();
            sw?.Dispose();
        }
    }

    enum Level
    {
        Trace, Debug, Info, Error
    }
}
