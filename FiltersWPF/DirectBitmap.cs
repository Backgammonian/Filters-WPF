using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FiltersWPF
{
    public class DirectBitmap
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, Color color)
        {
            int index = x + (y * Width);
            int col = color.ToArgb();

            Bits[index] = col;
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        private bool Contains(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public void DrawPoint(int x, int y, Color color)
        {
            if (Contains(x, y))
            {
                SetPixel(x, y, color);
            }
        }

        public void Clear(Color color)
        {
            using (var gr = Graphics.FromImage(Bitmap))
            {
                gr.Clear(color);
            }
        }

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }

        public DirectBitmap Clone()
        {
            var bm = new DirectBitmap(Width, Height);
            using (var gr = Graphics.FromImage(bm.Bitmap))
            {
                gr.DrawImageUnscaled(Bitmap, 0, 0, Width, Height);
            }

            return bm;
        }

        public void DrawImage(Bitmap image)
        {
            using var gr = Graphics.FromImage(Bitmap);
            var destinationRectangle = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
            var sourceRectangle = new Rectangle(0, 0, image.Width, image.Height);
            gr.DrawImage(image, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
        }
    }
}