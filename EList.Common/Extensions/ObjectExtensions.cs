using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EList.Common.Extensions
{
    public static class ObjectExtensions
    {
        public static bool HasStringValue(this object input) => input?.ToString().IsNullOrWhiteSpace() is false;
        public static bool HasStringValue(this string input) => input?.IsNullOrWhiteSpace() is false;

        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }
    }
}
