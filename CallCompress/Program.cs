using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallCompress
{
    internal class Program
    {
#if DEBUG
        private static bool enableLog = false;
#endif

        public static void Main(string[] args)
        {
#if DEBUG
            enableLog = true;
            //System.Threading.Thread.Sleep(5000);
#endif
            mainDynamic();
        }

        public static void mainCMD()
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            info.FileName = System.Environment.GetEnvironmentVariable("ComSpec");
            info.Arguments = "/c";
            string[] commandLines = Environment.GetCommandLineArgs();
            string replaceText = "\"" + commandLines[0] + "\"";
            string commandLine = Environment.CommandLine;
            string arguments = commandLine.Replace(replaceText, "");
            info.Arguments += arguments;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            var pro = System.Diagnostics.Process.Start(info);
        }

        public static void mainDynamic()
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            string[] commandLines = Environment.GetCommandLineArgs();
            StringBuilder argsbuild = new StringBuilder();
            if (commandLines.Count() > 2)
            {
                info.FileName = commandLines[1];
                foreach (string line in commandLines.Skip(2))
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(line, "\\s"))
                    {
                        argsbuild.Append(string.Format("\"{0}\" ", line));
                    }
                    else
                    {
                        argsbuild.Append(line + " ");
                    }
                }
                argsbuild.Remove(argsbuild.Length - 1, 1);
                info.Arguments = argsbuild.ToString();
                info.UseShellExecute = false;
                info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                var pro = System.Diagnostics.Process.Start(info);
            }
#if DEBUG
            if (enableLog)
            {
                System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "lastCallLog.log", string.Format("info.FileName:{0}\n" +
                    "info.Arguments:{1}", info.FileName, info.Arguments));
            }
#endif
        }
    }
}
