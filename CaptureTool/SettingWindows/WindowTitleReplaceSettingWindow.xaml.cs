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
using static CaptureTool.Settings;

namespace CaptureTool.SettingWindows
{
    /// <summary>
    /// WindowTitleReplaceSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class WindowTitleReplaceSettingWindow : Window
    {
        private Settings settings;
        private WindowTitleReplaceSettingWindowView view;

        public WindowTitleReplaceSettingWindow(Settings settings)
        {
            InitializeComponent();
            view = new WindowTitleReplaceSettingWindowView() { WindowTitleReplaceSettings = settings.WindowTitleReplaceSettings };
            DataContext = view;
            this.settings = settings;
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (settings != null && settings.WindowTitleReplaceSettings != null)
                {
                    if (sender is Button button && button.DataContext is Settings.WindowTitleReplaceSetting wtrSetting)
                    {
                        settings.WindowTitleReplaceSettings.Remove(wtrSetting);
                    }
                }
            }
            catch (Exception ex)
            {
                OverlayDialog overlayDialog = new OverlayDialog(ex.Message)
                {
                    Owner = this
                };
                overlayDialog.Show();

            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (settings != null && settings.WindowTitleReplaceSettings != null)
                {
                    settings.WindowTitleReplaceSettings.Add(new Settings.WindowTitleReplaceSetting());
                }
            }
            catch (Exception ex)
            {
                OverlayDialog overlayDialog = new OverlayDialog(ex.Message)
                {
                    Owner = this
                };
                overlayDialog.Show();

            }
        }
        

    public class WindowTitleReplaceSettingWindowView
    {
        public System.Collections.ObjectModel.ObservableCollection<WindowTitleReplaceSetting> WindowTitleReplaceSettings { get; set; }


        //public double pColWidth
        //{
        //    get
        //    {
        //        return pCol - 20;
        //    }
        //}
        //public double pColHeight
        //{
        //    get
        //    {
        //        return _pColHeight - 20;
        //    }
        //}

        //public double replaceColWidth
        //{
        //    get
        //    {
        //        return _replaceColWidth - 20;
        //    }
        //}
        //public double replaceColHeight
        //{
        //    get
        //    {
        //        return _replaceColHeight - 20;
        //    }
        //}
    }
    }
}
