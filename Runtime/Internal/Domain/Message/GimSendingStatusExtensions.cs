namespace Gamania.GIMChat.Internal.Domain.Message
{
    internal static class GimSendingStatusExtensions
    {
        public static bool IsTerminal(this GimSendingStatus status)
        {
            return status == GimSendingStatus.Succeeded || status == GimSendingStatus.Canceled;
        }

        public static bool CanTransitionTo(this GimSendingStatus current, GimSendingStatus target)
        {
            if (current == target) return false;

            return current switch
            {
                GimSendingStatus.None => target == GimSendingStatus.Pending || target == GimSendingStatus.Canceled,
                GimSendingStatus.Pending => target == GimSendingStatus.Succeeded || target == GimSendingStatus.Failed || target == GimSendingStatus.Canceled,
                GimSendingStatus.Failed => target == GimSendingStatus.Pending || target == GimSendingStatus.Canceled, // Retry or cancel
                GimSendingStatus.Succeeded => false, // Terminal state
                GimSendingStatus.Canceled => false, // Terminal state
                _ => false
            };
        }
    }
}
