namespace EList.FilestorageClient
{
    /// <summary>
    /// Модерационный доступ: Active — обычные правила visibility; Blocked — только service-token.
    /// </summary>
    public enum FileAccessStatus : short
    {
        Active = 0,
        Blocked = 1
    }
}
