using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace FiltersWPF
{
    public static class DirectBitmapExtensions
    {
        public static BitmapImage ToBitmapImage(this DirectBitmap directBitmap)
        {
            using MemoryStream memory = new MemoryStream();
            directBitmap.Bitmap.Save(memory, ImageFormat.Bmp);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }
}
