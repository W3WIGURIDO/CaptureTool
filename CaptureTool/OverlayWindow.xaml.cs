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
using System.Windows.Media.Animation;
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
                // [変更] ウィンドウサイズをコンテンツに合わせる
                Width = value;
            }
        }

        public double ImageGridHeight
        {
            set
            {
                imageGrid.Height = value;
                // [変更] ウィンドウサイズをコンテンツに合わせる
                Height = value;
            }
        }

        public int OverlayTime { get; set; } = 3000;

        // [変更] 最大化廃止に伴い、配置先をgridViewのAlignmentからウィンドウ座標に変更
        private HorizontalAlignment _overlayHorizontalAlignment = HorizontalAlignment.Left;
        public HorizontalAlignment OverlayHorizontalAlignment
        {
            get => _overlayHorizontalAlignment;
            set => _overlayHorizontalAlignment = value;
        }

        private VerticalAlignment _overlayVerticalAlignment = VerticalAlignment.Top;
        public VerticalAlignment OverlayVerticalAlignment
        {
            get => _overlayVerticalAlignment;
            set => _overlayVerticalAlignment = value;
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
        private const int WS_EX_NOACTIVATE = 0x08000000;
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
                    extendStyle |= WS_EX_NOACTIVATE;  // フォーカス移動を防ぐ
                }
                else
                {
                    extendStyle &= ~WS_EX_TRANSPARENT;
                    extendStyle &= ~WS_EX_NOACTIVATE;  // フォーカス移動を防ぐ
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
                Dispatcher.Invoke(() => { OpacityAnimation(); });
                Thread.Sleep(1000);
                if (ClosingReady)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ImageSource = null;
                        Close();
                        MainProcess.prevOverlayWindow = null;
                    }, System.Windows.Threading.DispatcherPriority.Send);
                }
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _ClosingReady = false;
        }

        private void OpacityAnimation()
        {
            var storyboard = new Storyboard();
            DoubleAnimation CreateDoubleAnimation()
            {
                var da = new DoubleAnimation();
                Storyboard.SetTarget(da, gridView);
                Storyboard.SetTargetProperty(da, new PropertyPath("(Opacity)"));
                storyboard.Children.Add(da);
                return da;
            }
            DoubleAnimation da1 = CreateDoubleAnimation();
            da1.To = 0;
            da1.Duration = TimeSpan.FromSeconds(1);

            // アニメーションを開始します
            storyboard.Begin();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // [変更] 最大化を廃止し、選択コーナーへの座標配置に変更
            // 全画面透過ウィンドウの最大化が D3D12 シェーダーコンパイルを誘発していたため
            PositionWindow();
        }

        private void PositionWindow()
        {
            // アクティブ画面を取得（カーソル位置のモニター）
            var screen = System.Windows.Forms.Screen.FromPoint(
                System.Windows.Forms.Cursor.Position);

            // DPI 変換係数を取得（マルチモニター・高DPI対応）
            var source = PresentationSource.FromVisual(this);
            double dpiX = source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
            double dpiY = source?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;

            // 作業領域（タスクバーを除く）を論理ピクセルに変換
            double screenLeft = screen.WorkingArea.Left / dpiX;
            double screenTop = screen.WorkingArea.Top / dpiY;
            double screenRight = screen.WorkingArea.Right / dpiX;
            double screenBottom = screen.WorkingArea.Bottom / dpiY;

            const double margin = 10;

            Left = _overlayHorizontalAlignment == HorizontalAlignment.Right
                ? screenRight - Width - margin
                : screenLeft + margin;

            Top = _overlayVerticalAlignment == VerticalAlignment.Bottom
                ? screenBottom - Height - margin
                : screenTop + margin;
        }
    }

    public class OverlayWindowDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _OverlayText = "スクリーンショット取得";
        public string OverlayText
        {
            get => _OverlayText;
            set
            {
                if (value != null)
                {
                    _OverlayText = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
