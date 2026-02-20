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
using System.Windows.Shapes;

namespace CaptureTool.SettingWindows
{
    /// <summary>
    /// FileNameComboSourceWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class FileNameComboSourceWindow : Window
    {
        public FileNameComboSourceWindow()
        {
            InitializeComponent();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
