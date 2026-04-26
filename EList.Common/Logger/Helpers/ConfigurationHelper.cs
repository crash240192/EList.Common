using NLog.Web;

namespace EList.Common.Logger.Helpers
{
    public static class ConfigurationHelper
    {
        public static void Load(string path)
        {
            NLogBuilder.ConfigureNLog(path);
        }
    }
}
