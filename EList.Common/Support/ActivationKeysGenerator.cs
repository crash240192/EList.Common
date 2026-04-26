using EList.Common.Configuration;

namespace EList.Common.Support
{
    public static class ActivationKeysGenerator
    {
        private const int MIN_LENGTH = 4;
        private const int MAX_LENGTH = 10;
        private const int DEFAULT_LENGTH = 6;
       
        public static string Generate(int? length = DEFAULT_LENGTH)
        {
            if (length == null)
                length = DEFAULT_LENGTH;

            if (length<MIN_LENGTH)
                length = MIN_LENGTH;

            if (length > MAX_LENGTH)
                length = MAX_LENGTH;

            var result = string.Empty;
            var random = new Random();

            for (int i = 0; i < length; i++)
            {
                result += random.Next(0, 9);
            }

            return result;
        }
    }
}
