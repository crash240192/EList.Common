using EList.Common.Configuration;

namespace EList.Common.Encryption
{
    public class EncryptionTool : IEncryptionTool
    {
        private readonly string salt;

        public EncryptionTool()
        {
            if (ConfigurationManager.AppSettings.Contains("encryption:salt"))
                salt = ConfigurationManager.AppSettings["encryption:salt"];
            else
                salt = string.Empty;
        }

        public string CalculateStringHash(string value)
        {
            var hash = EncryptionUtility.EncryptMD5(value);

            if (!string.IsNullOrWhiteSpace(salt))
                hash = EncryptionUtility.EncryptMD5(hash + salt);

            return hash;
        }

        //public string EncryptUserData(UserHashData data)
        //{
        //    var json = JsonConvert.SerializeObject(data);
        //    return EncryptionUtility.EncryptDESToBase64(json);
        //}

        //public UserHashData DecryptUserData(string hash)
        //{
        //    var json = EncryptionUtility.DecryptDESFromBase64(hash);
        //    return JsonConvert.DeserializeObject<UserHashData>(json);
        //}
    }
}
