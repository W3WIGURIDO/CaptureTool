using System;
using System.Text;
using System.Windows;

namespace CaptureTool
{
    public class Logger : IDisposable
    {
        private static readonly Encoding enc = Encoding.GetEncoding("Shift_JIS");
        private readonly object _lockObj = new object();

        // アプリ全体で共有するインスタンス
        public static Logger Default { get; private set; }

        public static void InitializeDefault(string path, int level)
        {
            Default = new Logger(path, level);
        }

        private Level _level;
        private System.IO.StreamWriter _sw = null;

        public Logger(string path, int level)
        {
            if (level < 0) level = 0;
            if (level > 3) level = 3;
            _level = (Level)level;
            // [2026-05-12 修正] AutoFlush=true でクラッシュ時もログが失われないようにする
            _sw = new System.IO.StreamWriter(path, true, enc) { AutoFlush = true };
        }

        private void WriteLog(string text, Level level)
        {
            // [2026-05-12 修正] lockでスレッドセーフにする
            lock (_lockObj)
            {
                try
                {
                    _sw?.WriteLine($"[{level}]<{DateTime.Now}>: {text}");
                }
                catch (Exception ex)
                {
                    // [2026-05-12 修正] ログ書き込み失敗時はMessageBoxではなくデバッグ出力のみ
                    System.Diagnostics.Debug.WriteLine($"Logger write failed: {ex.Message}");
                }
            }
        }

        // [2026-05-12 修正] 判定を >= に修正
        // 修正前: Level.Error <= level (level=0のときErrorが出力されないバグ)
        // 修正後: level >= Level.Error (levelが閾値以上のメッセージを出力する)
        public void Error(string text) { if (_level >= Level.Error) WriteLog(text, Level.Error); }
        public void Info(string text) { if (_level >= Level.Info) WriteLog(text, Level.Info); }
        public void Debug(string text) { if (_level >= Level.Debug) WriteLog(text, Level.Debug); }
        public void Trace(string text) { if (_level >= Level.Trace) WriteLog(text, Level.Trace); }

        public void Dispose()
        {
            lock (_lockObj)
            {
                _sw?.Close();
                _sw?.Dispose();
                _sw = null;
            }
        }
    }

    enum Level
    {
        Trace, Debug, Info, Error
    }
}