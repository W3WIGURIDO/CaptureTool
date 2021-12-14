using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CaptureTool
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            Shutdown();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //Process process = System.Diagnostics.Process.GetCurrentProcess();
        }
    }
}
