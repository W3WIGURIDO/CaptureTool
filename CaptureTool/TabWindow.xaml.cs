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
    /// TabWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TabWindow : Window
    {
        private MainInstance mainInstance;
        public TabWindow(MainInstance mainInstance)
        {
            InitializeComponent();
            this.mainInstance = mainInstance;
            mainInstance.mainGrid.Margin = new Thickness(5);
            Content = mainInstance;
            Title = (mainInstance.settings.TabNumber + 1).ToString();
            mainInstance.viewWindowButton.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.GetMainWindowDataContext().UserControls.Remove(mainInstance);
            mainInstance.settings.HotKeySettings.DisposeHotKeys();
        }
    }
}
