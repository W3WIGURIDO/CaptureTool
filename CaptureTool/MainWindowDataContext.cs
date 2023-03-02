using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace CaptureTool
{
    public class MainWindowDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private readonly System.Collections.ObjectModel.ObservableCollection<MainInstance> _UserControls = new System.Collections.ObjectModel.ObservableCollection<MainInstance>();
        public System.Collections.ObjectModel.ObservableCollection<MainInstance> UserControls
        {
            get => _UserControls;
        }

        private readonly System.Collections.ObjectModel.ObservableCollection<TabItem> _TabItems = new System.Collections.ObjectModel.ObservableCollection<TabItem>();
        public System.Collections.ObjectModel.ObservableCollection<TabItem> TabItems
        {
            get => _TabItems;
        }

        public bool TopMost => (UserControls.Count > TabSelectedIndex && TabSelectedIndex > -1) ? UserControls[TabSelectedIndex].settings.TopMost : false;

        public bool IsEnabledRemoveTabButton => UserControls.Count > 0;

        public void AddTab(int index)
        {
            MainInstance mainInstance = new MainInstance(index);
            foreach (MainInstance otherInstance in UserControls)
            {
                if (otherInstance.settings.ContainsHotKeyPair(mainInstance.settings.PreKey, mainInstance.settings.Key))
                {
                    mainInstance.settings.WindowCaptureEnabled = false;
                }
                if (otherInstance.settings.ContainsHotKeyPair(mainInstance.settings.ScreenPreKey, mainInstance.settings.ScreenKey))
                {
                    mainInstance.settings.ScreenCaptureEnabled = false;
                }
                if (otherInstance.settings.ContainsHotKeyPair(mainInstance.settings.SelectPreKey, mainInstance.settings.SelectKey))
                {
                    mainInstance.settings.SelectEnabled = false;
                }
            }
            mainInstance.settings.HotKeySettings.StartHotKey();
            UserControls.Add(mainInstance);

            TabItem tabItem = new TabItem();
            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            stackPanel.Children.Add(new TextBlock() { Text = (index + 1).ToString(), Margin = new System.Windows.Thickness(0, 0, 4, 0) });
            Button closeButton = new Button() { Content = new TextBlock() { Text = "✖" } };
            closeButton.Click += (sender, e) =>
            {
                RemoveTab(mainInstance, tabItem);
            };
            stackPanel.Children.Add(closeButton);
            tabItem.Content = mainInstance;
            tabItem.Header = stackPanel;
            TabItems.Add(tabItem);
            LastTabNumber = index;
        }

        public void RemoveTab(MainInstance mainInstance, TabItem tabItem)
        {
            UserControls.Remove(mainInstance);
            TabItems.Remove(tabItem);
            mainInstance.settings.HotKeySettings.DisposeHotKeys();
        }

        private List<System.Windows.Window> _TabWindows = new List<System.Windows.Window>();
        public List<System.Windows.Window> TabWindows => _TabWindows;

        public void ViewWindowTab()
        {
            var mainInstance = UserControls[TabSelectedIndex];
            TabWindow tabWindow = new TabWindow(mainInstance);
            TabItems.RemoveAt(TabSelectedIndex);
            TabWindows.Add(tabWindow);
            tabWindow.Show();
        }

        public int TabSelectedIndex { get; set; }
        public RoutedCommand RefFolderCom { get; set; } = new RoutedCommand();

        public int LastTabNumber { get; set; } = 0;
    }
}
