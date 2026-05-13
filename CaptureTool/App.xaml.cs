using System;
using System.Windows;

namespace CaptureTool
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // [2026-05-12 追加] アプリ起動時にLoggerを初期化して全体で共有する
            Logger.InitializeDefault(AppDomain.CurrentDomain.BaseDirectory + "log.txt", 3);

            // [2026-05-12 追加] UIスレッドの未処理例外をログに記録する
            DispatcherUnhandledException += (s, ex) =>
            {
                Logger.Default?.Error("DispatcherUnhandledException: " + ex.Exception);
            };

            // [2026-05-12 追加] Task.Run内などの未処理例外をログに記録する
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, ex) =>
            {
                Logger.Default?.Error("UnobservedTaskException: " + ex.Exception);
                ex.SetObserved();
            };

            // [2026-05-12 追加] CLRレベルの未処理例外をログに記録する
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                Logger.Default?.Error($"UnhandledException (IsTerminating={ex.IsTerminating}): {ex.ExceptionObject}");
            };
        }

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            Shutdown();
        }

        // [2026-05-12 追加] アプリ終了時にLoggerを破棄する
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Logger.Default?.Dispose();
        }
    }
}