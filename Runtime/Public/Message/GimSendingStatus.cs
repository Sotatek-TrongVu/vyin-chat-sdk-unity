namespace Gamania.GIMChat
{
    /// <summary>
    /// Public message sending status for SDK consumers.
    /// </summary>
    public enum GimSendingStatus
    {
        None = 0,
        Pending = 1,
        Succeeded = 2,
        Failed = 3,
        Canceled = 4
    }
}
