using System.Collections.Generic;
using System.Globalization;

namespace DKS_HotelManager.Helpers
{
    public class PayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var comparer = CompareInfo.GetCompareInfo("en-US");
            return comparer.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
