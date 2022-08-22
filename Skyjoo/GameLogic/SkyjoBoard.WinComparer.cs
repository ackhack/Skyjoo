using System.Collections.Generic;

namespace Skyjoo.GameLogic
{
    public partial class SkyjoBoard
    {
        internal class WinComparer : IComparer<int[]>
        {
            public int Compare(int[] x, int[] y)
            {
                if (x.Length != 2 || y.Length != 2) return 0;
                return x[1] - y[1];
            }
        }
    }
}