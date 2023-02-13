using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Imaging;
using Aspose.Imaging.ImageOptions;

namespace CaptureTool
{
    class ImageCompression
    {
        public static void SavePng(string path, System.Drawing.Bitmap bitmap, int compressionLevel)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.SetLength(0);
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            // ファイル(またはストリーム)から画像をロードする
            using (Image image = Image.Load(ms))
            {
                // PngOptionsのインスタンスを作成し、CompressionLevelを設定し、結果をディスクに保存します
                PngOptions options = new PngOptions();
                options.CompressionLevel = compressionLevel;
                image.Save(path, options);
            }

        }

    }
}
