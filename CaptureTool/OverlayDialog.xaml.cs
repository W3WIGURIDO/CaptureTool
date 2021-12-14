using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// OverlayDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class OverlayDialog : Window
    {
        public OverlayDialog()
        {
            InitializeComponent();
        }

        public OverlayDialog(string text)
        {
            InitializeComponent();
            viewingText.Text = text;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Left = Owner.Left + Owner.Width / 2 - Width / 2;
            Top = Owner.Top + Owner.Height / 2 - Height / 2;
            Task.Run(() =>
            {
                Thread.Sleep(1500);
                bool isContinue = true;
                while (isContinue)
                {
                    gridView.Dispatcher.Invoke(() =>
                    {
                        gridView.Opacity = gridView.Opacity - 0.1;
                        if (gridView.Opacity <= 0)
                        {
                            isContinue = false;
                        }
                    });
                    Thread.Sleep(100);
                }
                Dispatcher.Invoke(() =>
                {
                    Close();
                });
            });
        }
    }
}
