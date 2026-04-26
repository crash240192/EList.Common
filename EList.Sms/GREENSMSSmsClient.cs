using EList.Common.Configuration;
using EList.Common.CorrelationId;
using EList.Common.HttpRestClient;
using EList.Common.Logger;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EList.Sms
{
    public class GREENSMSSmsClient : ISmsClient
    {
        #region NLog
        private static readonly ILoggerWrapper logger = new NLogLoggerWrapper(LogManager.GetCurrentClassLogger());
        private const string LOGGER_NAME = "EList.Sms.SMSRUSmsClient.";
        #endregion

        private readonly string apiId;
        private readonly string baseUrl;
        private readonly string user;
        private readonly string pass;
        private readonly string sender;

        private readonly ICorrelationIdProvider _correlationIdProvider;

        public GREENSMSSmsClient(ICorrelationIdProvider correlationIdProvider)
        {
            baseUrl = ConfigurationManager.AppSettings.Contains("notificationService:sms:baseUrl")
                ? ConfigurationManager.AppSettings["notificationService:sms:baseUrl"]
                : throw new MissingFieldException("В файле конфигурации отсутствует apiKey по маршруту $.notificationService.sms.baseUrl");

            user = ConfigurationManager.AppSettings.Contains("notificationService:sms:user")
                ? ConfigurationManager.AppSettings["notificationService:sms:user"]
                : string.Empty;

            pass = ConfigurationManager.AppSettings.Contains("notificationService:sms:pass")
                ? ConfigurationManager.AppSettings["notificationService:sms:pass"]
                : string.Empty;

            sender = ConfigurationManager.AppSettings.Contains("notificationService:sms:sender")
                ? ConfigurationManager.AppSettings["notificationService:sms:sender"]
                : string.Empty;

            _correlationIdProvider = correlationIdProvider;
        }

        public async Task SendSmsAsync(string addressee, string message)
        {
            #region logger init & method started
            var correlationId = _correlationIdProvider.Get();
            var methodName = nameof(SendSmsAsync);
            var loggerName = $"{LOGGER_NAME}{methodName}";
            var execTime = Stopwatch.StartNew();

            logger.Debug(correlationId, null, methodName, $"Method started", null);
            #endregion

            var dict = new Dictionary<string, string>();
            dict.Add("user", user);
            dict.Add("pass", pass);
            dict.Add("to", addressee);
            dict.Add("txt", message);
            dict.Add("from", sender);
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, baseUrl) { Content = new FormUrlEncodedContent(dict) };
            var res = await client.SendAsync(req);

            logger.Debug(correlationId, null, methodName, $"Method finished", null, execTime.Elapsed);
        }
    }
}
