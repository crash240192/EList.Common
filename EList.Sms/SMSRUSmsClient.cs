using EList.Common.Configuration;
using EList.Common.CorrelationId;
using EList.Common.HttpRestClient;
using EList.Common.Logger;
using NLog;
using System.Diagnostics;

namespace EList.Sms
{
    public class SMSRUSmsClient : ISmsClient
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

        public SMSRUSmsClient(ICorrelationIdProvider correlationIdProvider)
        {
            apiId = ConfigurationManager.AppSettings.Contains("notificationService:sms:apiId")
                ? ConfigurationManager.AppSettings["notificationService:sms:apiId"]
                : throw new MissingFieldException("В файле конфигурации отсутствует apiKey по маршруту $.notificationService.sms.apiId");

            baseUrl = ConfigurationManager.AppSettings.Contains("notificationService:sms:baseUrl")
                ? ConfigurationManager.AppSettings["notificationService:sms:baseUrl"]
                : throw new MissingFieldException("В файле конфигурации отсутствует apiKey по маршруту $.notificationService.sms.baseUrl");

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

            var client = new HttpRestClient(baseUrl);
            var resultUrl = $"{baseUrl}?api_id={apiId}&to={addressee}&msg={message}&json=1";
            await client.GetAsync(resultUrl, correlationId);

            logger.Debug(correlationId, null, methodName, $"Method finished", null, execTime.Elapsed);
        }
    }
}