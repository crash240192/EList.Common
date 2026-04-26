
namespace EList.Common.Encryption
{
    public interface IEncryptionTool
    {
        string CalculateStringHash(string password);
        //UserHashData DecryptUserData(string hash);
        //string EncryptUserData(UserHashData data);
    }
}
