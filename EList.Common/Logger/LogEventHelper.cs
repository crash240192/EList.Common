using NLog;
using EList.Common.Logger.Containers;
using EList.Common.Logger.Helpers;

namespace EList.Common.Logger
{
    public static class LogEventHelper
    {
        #region consts
        private const int MAX_COUNT_OF_EVENTS_BEFORE_DISPOSE = 1;
        private const string TOO_MANY_EVENTS_BEFORE_DISPOSE_ERROR_MESSAGE = "Количество незалогированных logEvent перед завершением метода больше {0}!";
        #endregion

        #region NLog section        
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly ILoggerWrapper logger = new NLogLoggerWrapper(log);
        #endregion

        #region public methods
        public static string CreateEvent(string correlationId, bool isAsync = false)
        {
            var eventId = Guid.NewGuid().ToString();
            CreateEventInternal(eventId, correlationId, isAsync);
            return eventId;
        }

        public static void CreateEvent(string eventId, string correlationId, bool isAsync = false)
        {
            CreateEventInternal(eventId, correlationId, isAsync);
        }

        public static void SetVariable(string eventId, LogEventVariable logEventVariable, object value, bool isAsync = false)
        {
            if (value == null)
                return;

            var containers = isAsync ? GetContainersFromStorageAsync() : GetContainersFromStorage();
            if (containers == null)
                return;

            if (containers.ContainsKey(eventId))
            {
                var container = containers[eventId];
                switch (logEventVariable)
                {
                    case LogEventVariable.ClientIp:
                        container.ClientIp = value.ToString();
                        break;
                    case LogEventVariable.CorrelationId:
                        container.CorrelationId = value.ToString();
                        break;
                    case LogEventVariable.Guid:
                        container.Guid = value.ToString();
                        break;
                    case LogEventVariable.LoggerName:
                        container.LoggerName = value.ToString();
                        break;
                    case LogEventVariable.LogLevel:
                        container.LogLevel = (Enums.LogLevel)value;
                        break;
                    case LogEventVariable.IncommingMessage:
                        container.IncommingMessage = value.ToString();
                        break;
                    case LogEventVariable.Message:
                        container.Message = value.ToString();
                        break;
                    case LogEventVariable.StartEventTime:
                        container.StartEventTime = value as DateTime?;
                        break;
                    case LogEventVariable.EndEventTime:
                        container.EndEventTime = value as DateTime?;
                        break;
                    case LogEventVariable.RequestId:
                        container.RequestId = value as RequestIdContainer;
                        break;
                }

                if (isAsync)
                {
                    UpdateStorageAsync(containers);
                }
                else
                {
                    UpdateStorage(containers);
                }
            }
        }

        public static void LogAndDispose(string eventId, bool isAsync = false)
        {
            var containers = isAsync ? GetContainersFromStorageAsync() : GetContainersFromStorage();
            if (containers == null)
                return;

            if (containers.ContainsKey(eventId))
            {
                var container = containers[eventId];
                Log(container);
                containers.Remove(eventId);

                if (isAsync)
                {
                    UpdateStorageAsync(containers);
                }
                else
                {
                    UpdateStorage(containers);
                }
            }
        }

        public static void LogAndDisposeAll(bool isAsync = false)
        {
            var containers = isAsync ? GetContainersFromStorageAsync() : GetContainersFromStorage();
            if (containers == null)
                return;

            if (containers.Count() > MAX_COUNT_OF_EVENTS_BEFORE_DISPOSE)
            {
                var container = containers.Values.ElementAt(0);
                var message = string.Format(TOO_MANY_EVENTS_BEFORE_DISPOSE_ERROR_MESSAGE, MAX_COUNT_OF_EVENTS_BEFORE_DISPOSE);

                logger.Warn(
                    container.CorrelationId,
                    container.Guid,
                    container.LoggerName,
                    message,
                    container.ClientIp);
            }

            var keys = new List<string>();
            keys.AddRange(containers.Keys);

            foreach (var key in keys)
            {
                LogAndDispose(key, isAsync);
            }

            if (isAsync)
            {
                ClearStorageAsync();
            }
            else
            {
                ClearStorage();
            }
        }

        public static bool IsEventExist(string eventId, bool isAsync = false)
        {
            var containers = isAsync ? GetContainersFromStorageAsync() : GetContainersFromStorage();
            return containers?.ContainsKey(eventId) ?? false;
        }

        public static object GetVariableValue(string eventId, LogEventVariable logEventVariable, bool isAsync = false)
        {
            var containers = isAsync ? GetContainersFromStorageAsync() : GetContainersFromStorage();
            if (containers == null)
                return null;

            if (containers.ContainsKey(eventId))
            {
                var container = containers[eventId];
                switch (logEventVariable)
                {
                    case LogEventVariable.ClientIp:
                        return container.ClientIp;

                    case LogEventVariable.CorrelationId:
                        return container.CorrelationId;

                    case LogEventVariable.EndEventTime:
                        return container.EndEventTime;

                    case LogEventVariable.Guid:
                        return container.Guid;

                    case LogEventVariable.LoggerName:
                        return container.LoggerName;

                    case LogEventVariable.LogLevel:
                        return container.LogLevel;

                    case LogEventVariable.IncommingMessage:
                        return container.IncommingMessage;

                    case LogEventVariable.Message:
                        return container.Message;

                    case LogEventVariable.StartEventTime:
                        return container.StartEventTime;
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static void Dispose(string eventId, bool isAsync = false)
        {
            var containers = isAsync ? GetContainersFromStorageAsync() : GetContainersFromStorage();
            if (containers == null)
                return;

            if (containers.ContainsKey(eventId))
            {
                var container = containers[eventId];                
                containers.Remove(eventId);

                if (isAsync)
                {
                    UpdateStorageAsync(containers);
                }
                else
                {
                    UpdateStorage(containers);
                }
            }
        }
        #endregion

        #region private methods
        private static void CreateEventInternal(string eventId, string correlationId, bool isAsync = false)
        {
            var containers = isAsync ? GetContainersFromStorageAsync() : GetContainersFromStorage();
            if (containers == null)
            {
                containers = new Dictionary<string, LogEventContainer>();
            }

            if (!containers.ContainsKey(eventId))
            {
                var container = LogEventContainerBuilder.Build(correlationId);
                containers.Add(eventId, container);

                if (isAsync)
                {
                    SetContainersToStorageAsync(containers);
                }
                else
                {
                    SetContainersToStorage(containers);
                }
            }
        }

        private static void Log(LogEventContainer container)
        {
            TimeSpan? exectTime = null;

            if (container.StartEventTime != default(DateTime) &&
                container.EndEventTime != default(DateTime) &&
                container.StartEventTime < container.EndEventTime)
            {
                exectTime = container.EndEventTime - container.StartEventTime;
            }

            logger.Write(
                container.LogLevel,
                container.RequestId,
                container.CorrelationId,
                container.Guid,
                container.LoggerName,
                container.IncommingMessage,
                container.Message,
                container.ClientIp,
                exectTime);
        }

        private static Dictionary<string, LogEventContainer> GetContainersFromStorage()
        {
            return ThreadStateHelper.Get<Dictionary<string, LogEventContainer>>();
        }

        private static void SetContainersToStorage(Dictionary<string, LogEventContainer> containers)
        {
            ThreadStateHelper.Put(containers);
        }

        private static void ClearStorage()
        {
            ThreadStateHelper.Remove();
        }

        private static void UpdateStorage(Dictionary<string, LogEventContainer> containers)
        {
            ThreadStateHelper.Update(containers);
        }
        
        private static Dictionary<string, LogEventContainer> GetContainersFromStorageAsync()
        {
            return AsyncThreadStateHelper.Get<Dictionary<string, LogEventContainer>>();
        }

        private static void SetContainersToStorageAsync(Dictionary<string, LogEventContainer> containers)
        {
            AsyncThreadStateHelper.Put(containers);
        }

        private static void ClearStorageAsync()
        {
            AsyncThreadStateHelper.Remove();
        }

        private static void UpdateStorageAsync(Dictionary<string, LogEventContainer> containers)
        {
            AsyncThreadStateHelper.Update(containers);
        }
        #endregion
    }
}