using NLog;

namespace EList.Common.Logger.Helpers
{
    internal static class AsyncThreadStateHelper
    {
        public static void Put<T>(T threadState)
        {
            NestedDiagnosticsLogicalContext.Push(threadState);
        }

        public static void Update<T>(T state)
        {
            Remove();
            Put(state);
        }

        public static T Get<T>()
        {
            var topObject = NestedDiagnosticsLogicalContext.PeekObject();

            if (topObject == null)
            {
                return default(T);
            }

            return (T)topObject;
        }

        public static void Remove()
        {
            NestedDiagnosticsLogicalContext.Pop();
        }
    }
}