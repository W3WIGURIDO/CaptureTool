using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// OverrayWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public ImageSource ImageSource
        {
            set
            {
                if (viewImage != null)
                {
                    viewImage.Source = value;
                }
            }
        }

        public double ImageGridWidth
        {
            set
            {
                imageGrid.Width = value;
            }
        }

        public double ImageGridHeight
        {
            set
            {
                imageGrid.Height = value;
            }
        }

        public int OverlayTime { get; set; } = 3000;

        public HorizontalAlignment OverlayHorizontalAlignment
        {
            get => gridView.HorizontalAlignment;
            set
            {
                gridView.HorizontalAlignment = value;
            }
        }

        public VerticalAlignment OverlayVerticalAlignment
        {
            get => gridView.VerticalAlignment;
            set
            {
                gridView.VerticalAlignment = value;
            }
        }

        private bool _ClosingReady = true;
        public bool ClosingReady { get => _ClosingReady; }

        #region DependencyProperties

        #region AltF4Cancel

        public bool AltF4Cancel
        {
            get { return (bool)GetValue(AltF4CancelProperty); }
            set { SetValue(AltF4CancelProperty, value); }
        }

        public static readonly DependencyProperty AltF4CancelProperty =
            DependencyProperty.Register(nameof(AltF4Cancel), typeof(bool), typeof(OverlayWindow), new PropertyMetadata(true));

        #endregion

        #region ShowSystemMenu

        public bool ShowSystemMenu
        {
            get { return (bool)GetValue(ShowSystemMenuProperty); }
            set { SetValue(ShowSystemMenuProperty, value); }
        }

        public static readonly DependencyProperty ShowSystemMenuProperty =
            DependencyProperty.Register(nameof(ShowSystemMenu), typeof(bool), typeof(OverlayWindow), new PropertyMetadata(false, ShowSystemMenuPrpertyChanged));

        private static void ShowSystemMenuPrpertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OverlayWindow window)
            {
                window.SetShowSystemMenu((bool)e.NewValue);
            }
        }

        #endregion

        #region ClickThrough

        public bool ClickThrough
        {
            get { return (bool)GetValue(ClickThroughProperty); }
            set { SetValue(ClickThroughProperty, value); }
        }

        public static readonly DependencyProperty ClickThroughProperty =
            DependencyProperty.Register(nameof(ClickThrough), typeof(bool), typeof(OverlayWindow), new PropertyMetadata(true, ClickThroughPropertyChanged));

        private static void ClickThroughPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OverlayWindow window)
            {
                window.SetClickThrough((bool)e.NewValue);
            }
        }

        #endregion

        #endregion

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

        public OverlayWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            this.SetShowSystemMenu(this.ShowSystemMenu);
            this.SetClickThrough(this.ClickThrough);
            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource hwndSource = HwndSource.FromHwnd(handle);
            hwndSource.AddHook(WndProc);
            base.OnSourceInitialized(e);
        }

        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr IParam, ref bool handled)
        {
            if (msg == WM_SYSKEYDOWN && wParam.ToInt32() == VK_F4)
            {
                if (this.AltF4Cancel)
                {
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        protected void SetShowSystemMenu(bool value)
        {
            try
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;
                int windowStyle = GetWindowLong(handle, GWL_STYLE);
                if (value)
                {
                    windowStyle |= WS_SYSMENU;
                }
                else
                {
                    windowStyle &= ~WS_SYSMENU;
                }
                SetWindowLong(handle, GWL_STYLE, windowStyle);
            }
            catch
            {

            }
        }

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

        private void GridView_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Thread.Sleep(OverlayTime);
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
                if (ClosingReady)
                {
                    Dispatcher.Invoke(() => { Close(); });
                }
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _ClosingReady = false;
        }
    }
}
