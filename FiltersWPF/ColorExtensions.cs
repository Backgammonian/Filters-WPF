using System;
using System.Drawing;

namespace FiltersWPF
{
    public static class ColorExtensions
    {
        public static int GetColorBrightness(this Color color)
        {
            var result = Convert.ToInt32(Math.Sqrt(color.R * color.R * 0.241 + color.G * color.G * 0.691 + color.B * color.B * 0.068));
            return result < 0 ? 0 : result > byte.MaxValue ? byte.MaxValue : result;
        }
    }
}
