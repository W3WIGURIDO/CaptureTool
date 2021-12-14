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
    /// MiniWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MiniWindow : Window
    {
        private Settings settings;
        private bool loadFinished = false;
        public IntPtr Handle { get; }

        public MiniWindow(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
            this.DataContext = settings;
            Handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        }

        private void FileNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (loadFinished)
            {
                settings.FileName = fileNameBox.Text;
                settings.NumberCount = 0;
            }
            MainProcess.CreateFileNameNumberCountButtons(settings.FileName, countButtonPanel, settings);
        }

        private void NumberResetClick(object sender, RoutedEventArgs e)
        {
            settings.NumberCount = 0;
            settings.NumberCountSave();
        }

        private void DigitsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.DigitsText = digitsTextBox.Text;
        }

        private void CountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(countTextBox.Text, out int result))
            {
                settings.NumberCount = result;
            }
            else
            {
                countTextBox.Text = 0.ToString();
            }
        }

        private void CountUpButton_Click(object sender, RoutedEventArgs e)
        {
            settings.NumberCount++;
        }

        private void CountDownButton_Click(object sender, RoutedEventArgs e)
        {
            settings.NumberCount--;
        }

        private void MiniModeCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (miniModeCheck.IsChecked == false)
            {
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadFinished = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow.ActiveWindow.ReturnFromMiniMode();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            settings.NumberCount = MainProcess.GetContinueFileName(settings);
        }
    }
}
