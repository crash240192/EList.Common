using EList.Common.CorrelationId;
using EList.Common.Logger;
using EList.Common.Threading;
using EList.Smtp.Models;
using MimeKit;
using MimeKit.Text;
using NLog;
using System.Diagnostics;

namespace EList.Smtp
{
    public class SmtpClientMailKit : ISmtpClient
    {
        #region NLog
        private static readonly ILoggerWrapper logger = new NLogLoggerWrapper(LogManager.GetCurrentClassLogger());
        private const string LOGGER_NAME = "EList.Smtp.SmtpClientMailKit.";
        #endregion

        #region private & .ctor
        private readonly SmtpConfiguration smtpConfiguration;
        private readonly ICorrelationIdProvider correlationIdProvider;

        public SmtpClientMailKit(ICorrelationIdProvider correlationIdProvider)
        {
            smtpConfiguration = SmtpConfiguration.GetConfiguration();

            this.correlationIdProvider = correlationIdProvider;
        }
        #endregion

        public void SendMessage(Message message)
        {
            var correlationId = correlationIdProvider.Get();

            SendMessage(correlationId, message);
        }

        public void SendMessage(string correlationId, Message message)
        {
            AsyncHelper.RunSync(() => SendMessageAsync(correlationId, message));
        }

        public async Task SendMessageAsync(string correlationId, Message message)
        {
            #region logger init & method started
            var methodName = nameof(SendMessageAsync);
            var loggerName = $"{LOGGER_NAME}{methodName}";
            var execTime = Stopwatch.StartNew();
            var meta = new Dictionary<string, object>()
            {
                { nameof(message), message },
                { "smtp", new { smtpConfiguration.Host, smtpConfiguration.Port,
                    smtpConfiguration.EnableSsl, smtpConfiguration.Login,
                    smtpConfiguration.SenderEmail, smtpConfiguration.SenderName } }
            };

            logger.Debug(correlationId, null, loggerName, null, $"{methodName} method started", meta: meta);
            #endregion

            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(smtpConfiguration.SenderName, smtpConfiguration.SenderEmail));
                emailMessage.To.Add(new MailboxAddress("", message.RecipientEmail));
                emailMessage.Subject = message.MessageSubject;
                emailMessage.Body = new TextPart(message.IsBodyHtml ? TextFormat.Html : TextFormat.Text)
                {
                    Text = message.MessageBody
                };

                var cancellationDelayTimeout = TimeSpan.FromSeconds(smtpConfiguration.Timeout);
                using (var cts = new CancellationTokenSource(cancellationDelayTimeout))
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync(smtpConfiguration.Host, smtpConfiguration.Port, smtpConfiguration.EnableSsl);

                    if (!string.IsNullOrWhiteSpace(smtpConfiguration.Login) && !string.IsNullOrWhiteSpace(smtpConfiguration.Password))
                    {
                        await client.AuthenticateAsync(smtpConfiguration.Login, smtpConfiguration.Password, cancellationToken: cts.Token);
                    }

                    var sendResult = await client.SendAsync(emailMessage, cancellationToken: cts.Token);

                    #region logger Receiving SMTP-server response
                    logger.Debug(correlationId, null, loggerName, null,
                        $"The SMTP-server returned a response: '{sendResult}'",
                        execTime: execTime.Elapsed, meta: meta);
                    #endregion

                    await client.DisconnectAsync(true, cancellationToken: cts.Token);
                }

                #region logger info
                logger.Info(correlationId, null, loggerName, null,
                    $"The message was successfully sent to the email: '{message.RecipientEmail}'",
                    execTime: execTime.Elapsed, meta: meta);
                #endregion

                #region logger method finished
                logger.Debug(correlationId, null, loggerName, null,
                    $"{methodName} method finished", execTime: execTime.Elapsed, meta: meta);
                #endregion
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    meta.Add("innerExMessage", ex.InnerException.Message);
                    meta.Add("innerStackTrace", ex.InnerException.StackTrace);
                }

                #region logger error
                var errorMessage = $"Failed to send mesasge: {ex.Message}";

                logger.Error(correlationId, null, loggerName, errorMessage, execTime.Elapsed, ex, meta: meta);

                throw new Exception(errorMessage, ex);
                #endregion
            }
        }
    }
}
