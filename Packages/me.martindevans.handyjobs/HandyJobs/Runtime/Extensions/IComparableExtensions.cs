using System;

namespace Packages.HandyJobs.Runtime.Extensions
{
    public static class IComparableExtensions
    {
        public static bool IsLessThan<T>(this T a, T b)
            where T : IComparable<T>
        {
            var comp = a.CompareTo(b);
            return comp < 0;
        }

        public static bool IsLessThanOrEqualTo<T>(this T a, T b)
            where T : IComparable<T>
        {
            var comp = a.CompareTo(b);
            return comp <= 0;
        }

        public static bool IsGreaterThan<T>(this T a, T b)
            where T : IComparable<T>
        {
            var comp = a.CompareTo(b);
            return comp > 0;
        }

        public static bool IsGreaterThanOrEqualTo<T>(this T a, T b)
            where T : IComparable<T>
        {
            var comp = a.CompareTo(b);
            return comp >= 0;
        }
    }
}
