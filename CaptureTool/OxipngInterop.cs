// OxipngInterop.cs
// .NET Framework 4.6 向け oxipng.dll P/Invoke ラッパー
//
// 使用方法:
//   1. oxipng.dll をビルドして C# プロジェクトの出力フォルダに配置する
//   2. このファイルをプロジェクトに追加する
//   3. OxipngOptimizer.Optimize()             … PNG byte[] を渡す場合
//      OxipngOptimizer.OptimizeBitmap()       … System.Drawing.Bitmap を渡す場合（同期）
//      OxipngOptimizer.OptimizeBitmapAsync()  … System.Drawing.Bitmap を渡す場合（非同期）

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OxipngInterop
{
    // =========================================================
    //  ネイティブ定義との対応構造体
    //  Rust 側の #[repr(C)] OxipngOptions と同一レイアウト
    // =========================================================

    /// <summary>
    /// oxipng 最適化オプション。
    /// Rust 側の <c>OxipngOptions</c> 構造体と同一メモリレイアウト。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct OxipngOptions
    {
        /// <summary>最適化レベル (0〜6)。デフォルト: 2</summary>
        public byte OptLevel;

        /// <summary>
        /// メタデータ除去モード
        /// 0=除去しない / 1=安全なものだけ除去(推奨) / 2=すべて除去
        /// </summary>
        public byte StripMode;

        /// <summary>アルファチャネル最適化 (0=無効, 1=有効)</summary>
        public byte OptimizeAlpha;

        /// <summary>CRCエラー無視 (0=無効, 1=有効)</summary>
        public byte FixErrors;

        /// <summary>サイズが増加しても強制出力 (0=無効, 1=有効)</summary>
        public byte Force;

        /// <summary>インターレース (0=解除, 1=適用, 2=維持)</summary>
        public byte Interlace;

        /// <summary>ビット深度削減 (0=しない, 1=する)</summary>
        public byte BitDepthReduction;

        /// <summary>カラータイプ削減 (0=しない, 1=する)</summary>
        public byte ColorTypeReduction;

        /// <summary>パレット削減 (0=しない, 1=する)</summary>
        public byte PaletteReduction;

        /// <summary>グレースケール削減 (0=しない, 1=する)</summary>
        public byte GrayscaleReduction;

        /// <summary>IDAT再圧縮 (0=しない, 1=する)</summary>
        public byte IdatRecoding;
    }

    // =========================================================
    //  ネイティブ関数宣言 (P/Invoke)
    // =========================================================

    internal static class OxipngNative
    {
        private const string DllName = "oxipng";

        /// <summary>デフォルトオプション構造体を取得する。</summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "oxipng_default_options")]
        internal static extern OxipngOptions DefaultOptions();

        /// <summary>
        /// メモリ上のPNGを最適化する（フルオプション版）。
        /// 戻り値のポインタは必ず <see cref="Free"/> で解放すること。
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "oxipng_optimize_mem")]
        internal static extern IntPtr OptimizeMem(
            byte[] data,
            UIntPtr dataLen,
            ref OxipngOptions options,
            out UIntPtr outLen);

        /// <summary>
        /// メモリ上のPNGを最適化する（シンプル版）。
        /// 戻り値のポインタは必ず <see cref="Free"/> で解放すること。
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "oxipng_optimize_mem_simple")]
        internal static extern IntPtr OptimizeMemSimple(
            byte[] data,
            UIntPtr dataLen,
            byte optLevel,
            byte stripSafe,
            byte optimizeAlpha,
            out UIntPtr outLen);

        /// <summary>
        /// Bitmap.LockBits の Scan0 ポインタを直接受け取り最適化PNGを生成する。
        /// PNG エンコード/デコードを省略し、ピクセルデータもコピーなしで渡す。
        /// 戻り値のポインタは必ず <see cref="Free"/> で解放すること。
        /// </summary>
        /// <param name="scan0">BitmapData.Scan0（視覚的先頭行のポインタ）</param>
        /// <param name="width">画像幅（ピクセル）</param>
        /// <param name="height">画像高さ（ピクセル）</param>
        /// <param name="stride">
        ///   BitmapData.Stride をそのまま渡す。
        ///   負値（上下反転レイアウト）も自動処理される。
        /// </param>
        /// <param name="pixelFmt">
        ///   0 = BGR  (Format24bppRgb)<br/>
        ///   1 = BGRA (Format32bppArgb / Format32bppPArgb)
        /// </param>
        /// <param name="options">最適化オプション</param>
        /// <param name="outLen">出力バイト長</param>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "oxipng_optimize_bitmap")]
        internal static extern IntPtr OptimizeBitmap(
            IntPtr scan0,
            uint width,
            uint height,
            int stride,
            byte pixelFmt,
            ref OxipngOptions options,
            out UIntPtr outLen);

        /// <summary>
        /// <see cref="OptimizeMem"/> / <see cref="OptimizeBitmap"/> が返したメモリを解放する。
        /// ptr が IntPtr.Zero の場合は何もしない（安全）。
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "oxipng_free")]
        internal static extern void Free(IntPtr ptr, UIntPtr len);

        /// <summary>oxipng バージョン文字列（null終端）へのポインタを返す。</summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "oxipng_version")]
        internal static extern IntPtr Version();
    }

    // =========================================================
    //  マネージドラッパー（C#アプリケーションが直接使うクラス）
    // =========================================================

    /// <summary>
    /// oxipng DLL のマネージドラッパー。
    /// すべての P/Invoke 呼び出しと unsafe なメモリ管理をカプセル化する。
    /// </summary>
    public static class OxipngOptimizer
    {
        // ── バージョン取得 ─────────────────────────────────────

        /// <summary>
        /// oxipng のバージョン文字列を返す。
        /// </summary>
        public static string GetVersion()
        {
            IntPtr ptr = OxipngNative.Version();
            return Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
        }

        // ── Bitmap 直接最適化 ─────────────────────────────────
        //
        //  処理フロー（PNG コーデック往復を完全排除）:
        //
        //    Bitmap.LockBits
        //      → Scan0 ポインタ直渡し（コピーなし）
        //        → [DLL] BGR/BGRA→RGB/RGBA 変換 + 最適化 + PNG エンコード
        //          → Marshal.Copy で byte[] に受け取り
        //
        // ─────────────────────────────────────────────────────

        /// <summary>
        /// <see cref="Bitmap"/> を直接最適化し、PNG バイト配列を返す（簡易オーバーロード）。
        /// <para>
        /// PNG エンコード/デコードおよびピクセルデータのコピーを省略するため、
        /// <see cref="Optimize(byte[], int, bool, bool)"/> より高速。
        /// </para>
        /// </summary>
        /// <param name="bitmap">最適化対象の Bitmap</param>
        /// <param name="optLevel">最適化レベル (0〜6)</param>
        /// <param name="stripSafe">安全なメタデータを除去するか</param>
        /// <param name="optimizeAlpha">アルファチャネルを最適化するか</param>
        /// <returns>最適化済みPNGバイト配列。失敗時は null。</returns>
        public static byte[] OptimizeBitmap(
            Bitmap bitmap,
            int optLevel = 2,
            bool stripSafe = true,
            bool optimizeAlpha = false)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            OxipngOptions opts = OxipngNative.DefaultOptions();
            opts.OptLevel = (byte)Math.Min(optLevel, 6);
            opts.StripMode = stripSafe ? (byte)1 : (byte)0;
            opts.OptimizeAlpha = optimizeAlpha ? (byte)1 : (byte)0;

            return OptimizeBitmap(bitmap, opts);
        }

        /// <summary>
        /// <see cref="Bitmap"/> を直接最適化し、PNG バイト配列を返す（フルオプション版）。
        /// <para>
        /// PNG エンコード/デコードおよびピクセルデータのコピーを省略するため、
        /// <see cref="Optimize(byte[], OxipngOptions)"/> より高速。
        /// </para>
        /// </summary>
        /// <param name="bitmap">最適化対象の Bitmap</param>
        /// <param name="options">最適化オプション</param>
        /// <returns>最適化済みPNGバイト配列。失敗時は null。</returns>
        public static byte[] OptimizeBitmap(Bitmap bitmap, OxipngOptions options)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            // PixelFormat を BGR / BGRA に解決する
            // 未知のフォーマットは Format32bppArgb（BGRA）に正規化してロック
            PixelFormat lockFmt;
            byte pixelFmt;
            ResolveFormat(bitmap.PixelFormat, out lockFmt, out pixelFmt);

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, lockFmt);
            try
            {
                UIntPtr outLen;
                IntPtr resultPtr = OxipngNative.OptimizeBitmap(
                    bmpData.Scan0,        // Scan0 ポインタを直接渡す（コピーなし）
                    (uint)bitmap.Width,
                    (uint)bitmap.Height,
                    bmpData.Stride,       // 負値（上下反転レイアウト）もそのまま渡す
                    pixelFmt,
                    ref options,
                    out outLen);

                return CopyAndFree(resultPtr, outLen);
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
        }

        // ── Bitmap 非同期最適化 ───────────────────────────────
        //
        //  処理フロー:
        //
        //    [呼び出しスレッド] Bitmap.Clone() … UI スレッドで完結させる
        //      → Task.Run
        //        → [別スレッド] LockBits → DLL → UnlockBits → clone.Dispose()
        //          → await で結果を受け取る
        //
        //  ポイント:
        //    Clone は呼び出しスレッド（UI スレッド）で行う。
        //    Task.Run 内で Clone すると UI スレッドの描画と競合する可能性があるため。
        //
        // ─────────────────────────────────────────────────────

        /// <summary>
        /// <see cref="Bitmap"/> を非同期で最適化し、PNG バイト配列を返す（簡易オーバーロード）。
        /// </summary>
        /// <param name="bitmap">最適化対象の Bitmap</param>
        /// <param name="optLevel">最適化レベル (0〜6)</param>
        /// <param name="stripSafe">安全なメタデータを除去するか</param>
        /// <param name="optimizeAlpha">アルファチャネルを最適化するか</param>
        /// <param name="cloneFirst">
        ///   <c>true</c>（デフォルト）: 呼び出しスレッドで Clone を作成してから処理する。
        ///   元の Bitmap を UI スレッドで描画・変更し続ける場合に指定する。<br/>
        ///   <c>false</c>: Clone を作成せず元の Bitmap を直接処理する（Clone コスト不要）。
        ///   タスク実行中に元の Bitmap へ一切アクセスしないことが呼び出し側の責務となる。
        /// </param>
        /// <returns>最適化済みPNGバイト配列のタスク。失敗時は null。</returns>
        public static Task<byte[]> OptimizeBitmapAsync(
            Bitmap bitmap,
            int optLevel = 2,
            bool stripSafe = true,
            bool optimizeAlpha = false,
            bool cloneFirst = true)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            OxipngOptions opts = OxipngNative.DefaultOptions();
            opts.OptLevel = (byte)Math.Min(optLevel, 6);
            opts.StripMode = stripSafe ? (byte)1 : (byte)0;
            opts.OptimizeAlpha = optimizeAlpha ? (byte)1 : (byte)0;

            return OptimizeBitmapAsync(bitmap, opts, cloneFirst);
        }

        /// <summary>
        /// <see cref="Bitmap"/> を非同期で最適化し、PNG バイト配列を返す（フルオプション版）。
        /// </summary>
        /// <param name="bitmap">最適化対象の Bitmap</param>
        /// <param name="options">最適化オプション</param>
        /// <param name="cloneFirst">
        ///   <c>true</c>（デフォルト）: 呼び出しスレッドで Clone を作成してから処理する。
        ///   元の Bitmap を UI スレッドで描画・変更し続ける場合に指定する。<br/>
        ///   <c>false</c>: Clone を作成せず元の Bitmap を直接処理する（Clone コスト不要）。
        ///   タスク実行中に元の Bitmap へ一切アクセスしないことが呼び出し側の責務となる。
        /// </param>
        /// <returns>最適化済みPNGバイト配列のタスク。失敗時は null。</returns>
        public static Task<byte[]> OptimizeBitmapAsync(
            Bitmap bitmap,
            OxipngOptions options,
            bool cloneFirst = true)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            if (cloneFirst)
            {
                // Clone は呼び出しスレッド（UI スレッド）で作成する。
                // Task.Run 内で Clone すると UI スレッドの描画と競合する可能性があるため。
                Bitmap clone = (Bitmap)bitmap.Clone();
                return Task.Run(() =>
                {
                    using (clone)
                        return OptimizeBitmap(clone, options);
                });
            }
            else
            {
                // cloneFirst=false: Clone コストを省略する。
                // タスク実行中、呼び出し側は bitmap に一切アクセスしないこと。
                return Task.Run(() => OptimizeBitmap(bitmap, options));
            }
        }

        // ── PNG byte[] 最適化（シンプル版） ──────────────────

        /// <summary>
        /// PNG バイト配列を最適化する（簡易オーバーロード）。
        /// </summary>
        /// <param name="pngData">最適化対象のPNGバイト配列</param>
        /// <param name="optLevel">最適化レベル (0〜6)</param>
        /// <param name="stripSafe">安全なメタデータを除去するか</param>
        /// <param name="optimizeAlpha">アルファチャネルを最適化するか</param>
        /// <returns>最適化済みPNGバイト配列。失敗時は null。</returns>
        public static byte[] Optimize(
            byte[] pngData,
            int optLevel = 2,
            bool stripSafe = true,
            bool optimizeAlpha = false)
        {
            if (pngData == null || pngData.Length == 0)
                throw new ArgumentException("pngData が空です。", nameof(pngData));

            UIntPtr outLen;
            IntPtr resultPtr = OxipngNative.OptimizeMemSimple(
                pngData,
                (UIntPtr)pngData.Length,
                (byte)Math.Min(optLevel, 6),
                stripSafe ? (byte)1 : (byte)0,
                optimizeAlpha ? (byte)1 : (byte)0,
                out outLen);

            return CopyAndFree(resultPtr, outLen);
        }

        // ── PNG byte[] 最適化（フルオプション版） ────────────

        /// <summary>
        /// PNG バイト配列を最適化する（フルオプション版）。
        /// </summary>
        /// <param name="pngData">最適化対象のPNGバイト配列</param>
        /// <param name="options">最適化オプション</param>
        /// <returns>最適化済みPNGバイト配列。失敗時は null。</returns>
        public static byte[] Optimize(byte[] pngData, OxipngOptions options)
        {
            if (pngData == null || pngData.Length == 0)
                throw new ArgumentException("pngData が空です。", nameof(pngData));

            UIntPtr outLen;
            IntPtr resultPtr = OxipngNative.OptimizeMem(
                pngData,
                (UIntPtr)pngData.Length,
                ref options,
                out outLen);

            return CopyAndFree(resultPtr, outLen);
        }

        // ── デフォルトオプション取得 ──────────────────────────

        /// <summary>デフォルトの最適化オプションを取得する。</summary>
        public static OxipngOptions GetDefaultOptions()
            => OxipngNative.DefaultOptions();

        // ── 内部ヘルパー ──────────────────────────────────────

        /// <summary>
        /// PixelFormat から LockBits 用フォーマットと pixel_fmt 値を決定する。
        /// <list type="bullet">
        ///   <item>Format24bppRgb                              → lockFmt=24bpp, pixelFmt=0 (BGR)</item>
        ///   <item>Format32bppArgb / PArgb / Rgb              → lockFmt=32bpp, pixelFmt=1 (BGRA)</item>
        ///   <item>その他（インデックスカラー等の未対応形式）→ Format32bppArgb に正規化</item>
        /// </list>
        /// </summary>
        private static void ResolveFormat(
            PixelFormat src,
            out PixelFormat lockFmt,
            out byte pixelFmt)
        {
            switch (src)
            {
                case PixelFormat.Format24bppRgb:
                    lockFmt = PixelFormat.Format24bppRgb;
                    pixelFmt = 0; // BGR
                    break;

                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    lockFmt = PixelFormat.Format32bppArgb;
                    pixelFmt = 1; // BGRA
                    break;

                default:
                    // インデックスカラー・16bit 等は 32bppArgb に正規化して LockBits する
                    lockFmt = PixelFormat.Format32bppArgb;
                    pixelFmt = 1; // BGRA
                    break;
            }
        }

        /// <summary>
        /// ネイティブポインタの内容をマネージドバイト配列にコピーしてから解放する。
        /// </summary>
        private static byte[] CopyAndFree(IntPtr ptr, UIntPtr nativeLen)
        {
            if (ptr == IntPtr.Zero)
                return null; // 最適化失敗（不正なPNGなど）

            int len = (int)(ulong)nativeLen; // ulong 経由で安全にキャスト
            try
            {
                var result = new byte[len];
                Marshal.Copy(ptr, result, 0, len);
                return result;
            }
            finally
            {
                // 成功・例外どちらでも必ず解放
                OxipngNative.Free(ptr, nativeLen);
            }
        }
    }
}