using System;

namespace FiltersWPF
{
    public static class RandomExtensions
    {
        public static int NextOdd(this Random random, int min, int max)
        {
            int ans = random.Next(min, max);
            if (ans % 2 == 1)
            {
                return ans;
            }
            else
            {
                if (ans + 1 <= max)
                {
                    return ans + 1;
                }
                else
                if (ans - 1 >= min)
                {
                    return ans - 1;
                }
                else
                {
                    return 1;
                }
            }
        }
    }
}
