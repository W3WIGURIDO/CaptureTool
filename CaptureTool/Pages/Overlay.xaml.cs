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

namespace CaptureTool.Pages
{
    /// <summary>
    /// Overlay.xaml の相互作用ロジック
    /// </summary>
    public partial class Overlay : UserControl
    {
        private Settings settings
        {
            get
            {
                if (DataContext is Settings dsettings)
                {
                    return dsettings;
                }
                else
                {
                    return null;
                }
            }
        }

        public Overlay()
        {
            InitializeComponent();
        }

        private void OverlayTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.OverlayTime = overlayTimeTextBox.Text;
        }
    }
}
