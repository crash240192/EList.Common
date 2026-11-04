namespace EList.Common.Logger
{
    /// <summary>
    /// correlationId для кода, выполняемого вне HTTP-запроса (фоновые задачи, обработчики сообщений шины)
    /// и для параллельных веток (TaskRunner), где нужен свой ambient id.
    /// Значение сохраняется между await внутри scope; параллельным задачам его нужно
    /// пробрасывать явно через <see cref="Begin"/> (или он унаследуется из ExecutionContext родителя).
    /// </summary>
    public static class CorrelationIdScope
    {
        public const string MESSAGE_HEADER_NAME = "CorrelationId";

        private static readonly AsyncLocal<string> current = new AsyncLocal<string>();

        public static string Current => current.Value;

        public static IDisposable Begin(string correlationId)
        {
            var previous = current.Value;
            current.Value = correlationId;
            return new RestoreOnDispose(previous);
        }

        private sealed class RestoreOnDispose : IDisposable
        {
            private readonly string previous;
            private bool disposed;

            public RestoreOnDispose(string previous) => this.previous = previous;

            public void Dispose()
            {
                if (disposed)
                    return;

                current.Value = previous;
                disposed = true;
            }
        }
    }
}
