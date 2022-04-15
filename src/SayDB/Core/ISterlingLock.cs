namespace Say32.DB.Core
{
    /// <summary>
    ///     Lock mechanism
    /// </summary>
    public interface ISayDBLock
    {
        object Lock { get; }
    }
}
