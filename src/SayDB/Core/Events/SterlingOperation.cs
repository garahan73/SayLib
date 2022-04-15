namespace Say32.DB.Core.Events
{
    /// <summary>
    ///     Operation in SayDB
    /// </summary>
    public enum DbOperation
    {
        Save,
        Load,
        Delete,
        Flush,
        Purge,
        Truncate
    }
}
