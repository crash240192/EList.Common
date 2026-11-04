namespace EList.Common.Logger
{
    /// <summary>
    /// Профиль форматирования цепочки исключений.
    /// </summary>
    public enum ExceptionFormatKind
    {
        /// <summary>
        /// Краткий текст для ответа клиенту (CommandResult).
        /// </summary>
        Client = 0,

        /// <summary>
        /// Полное дерево для логов.
        /// </summary>
        Diagnostic = 1
    }
}
