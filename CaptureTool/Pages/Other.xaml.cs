using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CaptureTool.SettingWindows;

namespace CaptureTool.Pages
{
    /// <summary>
    /// Other.xaml の相互作用ロジック
    /// </summary>
    public partial class Other : UserControl
    {
        public Other()
        {
            InitializeComponent();
        }

        private void IpHostTransPathButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog cofd = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "ファイルを選択してください",
            };
            if (cofd.ShowDialog() != true)
            {
                return;
            }

            // FileNameで選択されたファイルを取得する
            if (DataContext is Settings settings)
            {
                if (settings != null)
                {
                    settings.IpHostTransHosts = cofd.FileName;
                }
            }
        }

        private void wtrButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Settings settings)
            {
                new WindowTitleReplaceSettingWindow(settings).ShowDialog();
            }
        }
    }
}
