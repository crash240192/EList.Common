using System.ComponentModel;

namespace EList.Common.Extensions
{
    public static class EnumExtensions
    {
        public static int Value<T>(this T value) where T : IConvertible
        {
            var type = typeof(T);

            if (!type.IsEnum)
                throw new InvalidEnumArgumentException("T is not an Enum type");

            return (int)(IConvertible)value;
        }

        public static string Description<T>(this T value) where T : Enum
        {
            var type = typeof(T);

            if (!type.IsEnum)
                throw new InvalidEnumArgumentException("T is not an Enum type");

            var member = type.GetMember(value.ToString());
            var attributes = member[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            var attribute = attributes.FirstOrDefault();
            if (attribute == null)
                return null;

            var description = ((DescriptionAttribute)attributes[0]).Description;
            return description;
        }

        public static T GetAttribute<T>(this Enum value)
            where T : Attribute
        {
            var type = value.GetType();
            var memInfo = type.GetMember(value.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0 ? (T)attributes[0] : null;
        }
    }
}
