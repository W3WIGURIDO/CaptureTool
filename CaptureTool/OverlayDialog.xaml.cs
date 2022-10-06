using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
        public static void ShowOverlayDialog(string text, Window owner)
        {
            OverlayDialog overlayDialog = new OverlayDialog(text);
            if(owner != null)
            {
                overlayDialog.Owner = owner;
            }
            overlayDialog.Show();
        }

        public OverlayDialog()
        {
            InitializeComponent();
        }

        public OverlayDialog(string text)
        {
            InitializeComponent();
            viewingText.Text = text;
        }

        #region const values

        private const int GWL_STYLE = (-16);
        private const int GWL_EXSTYLE = (-20);
        private const int WS_SYSMENU = 0x00080000;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int VK_F4 = 0x73;

        #endregion

        #region Win32Apis

        [DllImport("user32")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwLong);

        #endregion

        #region ClickThrough

        public bool ClickThrough
        {
            get { return (bool)GetValue(ClickThroughProperty); }
            set { SetValue(ClickThroughProperty, value); }
        }

        public static readonly DependencyProperty ClickThroughProperty =
            DependencyProperty.Register(nameof(ClickThrough), typeof(bool), typeof(OverlayDialog), new PropertyMetadata(true, ClickThroughPropertyChanged));

        private static void ClickThroughPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OverlayDialog window)
            {
                window.SetClickThrough((bool)e.NewValue);
            }
        }

        #endregion

        protected void SetClickThrough(bool value)
        {
            try
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;
                int extendStyle = GetWindowLong(handle, GWL_EXSTYLE);
                if (value)
                {
                    extendStyle |= WS_EX_TRANSPARENT;
                }
                else
                {
                    extendStyle &= ~WS_EX_TRANSPARENT;
                }
                SetWindowLong(handle, GWL_EXSTYLE, extendStyle);
            }
            catch
            {

            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            this.SetClickThrough(this.ClickThrough);
            base.OnSourceInitialized(e);
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
