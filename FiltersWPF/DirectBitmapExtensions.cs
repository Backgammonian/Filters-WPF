using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace FiltersWPF
{
    public static class DirectBitmapExtensions
    {
        public static BitmapImage ToBitmapImage(this DirectBitmap directBitmap)
        {
            using MemoryStream memory = new MemoryStream();
            directBitmap.Bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        private static (int[], int[], int[], int[], int[]) GetColorData(this DirectBitmap directBitmap)
        {
            var brightnesses = new int[256];
            var reds = new int[256];
            var greens = new int[256];
            var blues = new int[256];
            var alphas = new int[256];

            Parallel.For(0, directBitmap.Width - 1, i =>
            {
                Parallel.For(0, directBitmap.Height - 1, j =>
                {
                    var color = directBitmap.GetPixel(i, j);
                    var brightness = color.GetColorBrightness();

                    brightnesses[brightness] += 1;
                    reds[color.R] += 1;
                    greens[color.G] += 1;
                    blues[color.B] += 1;
                    alphas[color.A] += 1;
                });
            });

            return (brightnesses, reds, greens, blues, alphas);
            
        }

        private static BitmapImage GetHistogram(int padding, int barScale, int height, int[] data, Color color)
        {
            var canvas = new DirectBitmap(padding * 2 + data.Length * barScale, padding * 2 + height);
            canvas.Clear(Color.White);

            var maxValue = 0;
            for (int i = 0; i < data.Length; i++)
            {
                maxValue = Math.Max(maxValue, data[i]);
            }

            var canvasActualHeight = canvas.Height - padding * 2;
            var canvasActualWidth = canvas.Width - padding * 2;

            var barIndex = 0;
            var numberOfBars = data.Length;
            var barSize = canvasActualWidth / (double)numberOfBars;

            for (int i = 0; i < data.Length; i++)
            {
                var value = data[i];
                var barHeight = canvasActualHeight * value / (double)maxValue;

                var x = padding + barIndex * barSize;
                var y = canvas.Height - barHeight - padding;

                canvas.FillRectangle((int)x, (int)y, (int)barSize, (int)barHeight, color);

                barIndex++;
            }

            canvas.DrawRectangle(0, 0, canvas.Width - 1, canvas.Height - 1, Color.Gray);
            var histogramImage = canvas.ToBitmapImage();
            canvas.Bitmap.Dispose();

            return histogramImage;
        }

        public static (BitmapImage, BitmapImage, BitmapImage, BitmapImage, BitmapImage) GetHistograms(this DirectBitmap directBitmap, int padding, int barScale, int height, (Color, Color, Color, Color, Color) colors)
        {
            var datas = directBitmap.GetColorData();
            var brightnessHistogram = GetHistogram(padding, barScale, height, datas.Item1, colors.Item1);
            var redHistogram = GetHistogram(padding, barScale, height, datas.Item2, colors.Item2);
            var greenHistogram = GetHistogram(padding, barScale, height, datas.Item3, colors.Item3);
            var blueHistogram = GetHistogram(padding, barScale, height, datas.Item4, colors.Item4);
            var alphaHistogram = GetHistogram(padding, barScale, height, datas.Item5, colors.Item5);

            return (brightnessHistogram, redHistogram, greenHistogram, blueHistogram, alphaHistogram);
        }
    }
}
