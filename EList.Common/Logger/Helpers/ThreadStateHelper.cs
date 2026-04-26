using NLog;

namespace EList.Common.Logger.Helpers
{
    internal static class ThreadStateHelper
    {
        public static void Put<T>(T threadState)
        {
            NestedDiagnosticsContext.Push(threadState);
        }

        public static void Update<T>(T state)
        {
            Remove();
            Put(state);
        }

        public static T Get<T>()
        {
            var topObject = NestedDiagnosticsContext.TopObject;

            if (topObject == null)
            {
                return default(T);
            }

            return (T)topObject;
        }

        public static void Remove()
        {
            NestedDiagnosticsContext.Pop();
        }
    }
}
