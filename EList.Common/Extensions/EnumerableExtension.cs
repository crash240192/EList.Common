namespace EList.Common.Extensions
{
    public static class EnumerableExtension
    {
        public static bool NullSafeAny<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }

        public static bool NullSafeAny<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source != null && source.Any(predicate);
        }
    }
}