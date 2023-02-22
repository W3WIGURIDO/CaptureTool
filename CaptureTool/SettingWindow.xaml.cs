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

namespace CaptureTool
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        private Settings settings;
        public SettingWindow(Settings settings, Window owner)
        {
            InitializeComponent();
            DataContext = settings;
            this.settings = settings;
            if (owner != null)
            {
                Left = owner.Left;
                Top = owner.Top;
            }
            settings.DefaultOrientation = Orientation.Vertical;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            settings.RaisePropertyChanged(nameof(settings.CompressSelect));
        }

        private void okSaveButton_Click(object sender, RoutedEventArgs e)
        {
            settings.SaveSettings();
            settings.HotKeySettings.ResetHotKey();
            Close();
            settings.RaisePropertyChanged(nameof(settings.CompressSelect));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            settings.DefaultOrientation = Orientation.Horizontal;
        }
    }
}
