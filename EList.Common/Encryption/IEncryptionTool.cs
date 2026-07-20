
namespace EList.Common.Encryption
{
    public interface IEncryptionTool
    {
        string CalculateStringHash(string value);
        //UserHashData DecryptUserData(string hash);
        //string EncryptUserData(UserHashData data);
    }
}
