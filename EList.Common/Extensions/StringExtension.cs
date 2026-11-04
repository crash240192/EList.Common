namespace EList.Common.Extensions
{
    internal static class StringExtension
    {
        public const string HtmlWhiteSpace = "&nbsp;";

        public static bool IsEmpty(this string value) => value is "";
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);
        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        public static string CapitalizeFirstLetter(this string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Value cannot be null");

            if (value == string.Empty)
                throw new ArgumentException("Value cannot be empty", nameof(value));

            if (value.Length == 1)
                return value.ToUpperInvariant();

            return value.Substring(0, 1).ToUpperInvariant() + value.Substring(1).ToLowerInvariant();
        }

        public static string ToCamelCase(this string s)
        {
            if (!string.IsNullOrWhiteSpace(s) && s.Length > 1)
            {
                return char.ToLowerInvariant(s[0]) + s.Substring(1);
            }
            else
            {
                return s.ToLowerInvariant();
            }
        }

        public static string Repeat(this string value, int count, string separator = "")
        {
            return string.Join(separator, Enumerable.Repeat(value, count));
        }

        /// <summary>
        /// Removes all whitespace characters (spaces, tabs, newlines) from the string.
        /// Returns original string if the input is null or empty.
        /// </summary>
        public static string RemoveWhiteSpaces(this string value)
        {
            return string.IsNullOrEmpty(value)
                ? value
                : new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }
    }
}
