using EList.Common.Configuration;

namespace EList.Smtp.Models
{
    public class SmtpConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public int Timeout { get; set; }

        public static SmtpConfiguration GetConfiguration()
        {
            return new SmtpConfiguration
            {
                Host = ConfigurationManager.AppSettings["notificationService:smtp:host"],
                Port = int.Parse(ConfigurationManager.AppSettings["notificationService:smtp:port"]),
                EnableSsl = bool.Parse(ConfigurationManager.AppSettings["notificationService:smtp:ssl"]),
                Login = ConfigurationManager.AppSettings["notificationService:smtp:login"],
                Password = ConfigurationManager.AppSettings["notificationService:smtp:password"],
                SenderEmail = ConfigurationManager.AppSettings["notificationService:smtp:senderEmail"],
                SenderName = ConfigurationManager.AppSettings["notificationService:smtp:senderName"],
                Timeout = int.Parse(ConfigurationManager.AppSettings["notificationService:smtp:timeout"])
            };
        }
    }
}
