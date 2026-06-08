// ScreenUtil.cs
// System.Windows.Forms.Screen / Cursor.Position の P/Invoke 代替実装
// WinForms 依存除去 フェーズ3（Screen）

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CaptureTool
{
    /// <summary>
    /// System.Windows.Forms.Screen の P/Invoke 代替。
    /// モニターハンドル（HMONITOR）で同一性を判定するため、
    /// AllScreens.IndexOf(activeDisplay) が正しく機能する。
    /// </summary>
    public sealed class ScreenInfo : IEquatable<ScreenInfo>
    {
        /// <summary>モニターハンドル（HMONITOR）</summary>
        public IntPtr Handle { get; }
        /// <summary>モニター全体の座標矩形（タスクバー含む）</summary>
        public Rectangle Bounds { get; }
        /// <summary>作業領域（タスクバーを除く）</summary>
        public Rectangle WorkingArea { get; }
        /// <summary>プライマリモニターか否か</summary>
        public bool Primary { get; }

        internal ScreenInfo(IntPtr handle, Rectangle bounds, Rectangle workingArea, bool primary)
        {
            Handle = handle;
            Bounds = bounds;
            WorkingArea = workingArea;
            Primary = primary;
        }

        public bool Equals(ScreenInfo other) => other != null && Handle == other.Handle;
        public override bool Equals(object obj) => Equals(obj as ScreenInfo);
        public override int GetHashCode() => Handle.GetHashCode();
    }

    /// <summary>
    /// System.Windows.Forms.Screen および System.Windows.Forms.Cursor.Position の
    /// P/Invoke 代替ユーティリティ。
    /// </summary>
    public static class ScreenUtil
    {
        #region P/Invoke

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(
            IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        private delegate bool MonitorEnumProc(
            IntPtr hMonitor, IntPtr hdcMonitor, ref NativeRect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int left, top, right, bottom;
            public Rectangle ToRectangle() => Rectangle.FromLTRB(left, top, right, bottom);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public uint cbSize;
            public NativeRect rcMonitor;
            public NativeRect rcWork;
            public uint dwFlags;
        }

        private const uint MONITORINFOF_PRIMARY = 0x00000001;
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(NativePoint pt, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint { public int X, Y; }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out NativePoint lpPoint);

        #endregion

        private static ScreenInfo BuildScreenInfo(IntPtr hMonitor)
        {
            var info = new MONITORINFO { cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO)) };
            if (!GetMonitorInfo(hMonitor, ref info)) return null;
            return new ScreenInfo(
                hMonitor,
                info.rcMonitor.ToRectangle(),
                info.rcWork.ToRectangle(),
                (info.dwFlags & MONITORINFOF_PRIMARY) != 0);
        }

        /// <summary>
        /// 接続中の全モニター情報を取得する。プライマリをインデックス 0 に配置する。
        /// System.Windows.Forms.Screen.AllScreens に相当。
        /// </summary>
        public static List<ScreenInfo> GetAllScreens()
        {
            var list = new List<ScreenInfo>();
            // MonitorEnumProc は ref パラメータを持つため明示的にデリゲート変数を使う
            var cb = new MonitorEnumProc((IntPtr hMon, IntPtr hDC, ref NativeRect lprc, IntPtr d) =>
            {
                var s = BuildScreenInfo(hMon);
                if (s != null) list.Add(s);
                return true;
            });
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, cb, IntPtr.Zero);

            // プライマリモニターをインデックス 0 に移動
            int idx = list.FindIndex(s => s.Primary);
            if (idx > 0)
            {
                var primary = list[idx];
                list.RemoveAt(idx);
                list.Insert(0, primary);
            }
            return list;
        }

        /// <summary>
        /// カーソル位置のモニター情報を返す。
        /// System.Windows.Forms.Screen.FromPoint(Cursor.Position) に相当。
        /// MONITOR_DEFAULTTONEAREST 指定のため null を返さない。
        /// </summary>
        public static ScreenInfo FromCursorPosition()
        {
            GetCursorPos(out NativePoint pt);
            IntPtr hMon = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
            return BuildScreenInfo(hMon);
        }

        /// <summary>
        /// カーソル位置を Point で返す。
        /// System.Windows.Forms.Cursor.Position に相当（WriteCursorToGrap 等で使用）。
        /// </summary>
        public static Point GetCursorPosition()
        {
            GetCursorPos(out NativePoint pt);
            return new Point(pt.X, pt.Y);
        }
    }
}