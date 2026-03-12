using System.Collections.Generic;

namespace Gamania.GIMChat
{
    /// <summary>
    /// Receives real-time and changelog-driven events from a <see cref="GimGroupChannelCollection"/>.
    /// All callbacks are dispatched on the Unity main thread via MainThreadDispatcher.
    ///
    /// Usage:
    /// <code>
    /// collection.Delegate = myHandler;   // weak-referenced internally
    /// </code>
    /// </summary>
    public interface IGimGroupChannelCollectionDelegate
    {
        /// <summary>
        /// New channels were added to the collection.
        /// Sources: first LoadMore, changelog sync, or a real-time join event.
        /// </summary>
        void OnChannelsAdded(
            GimGroupChannelCollection collection,
            GimChannelContext context,
            IReadOnlyList<GimGroupChannel> addedChannels);

        /// <summary>
        /// Existing channels in the collection were updated.
        /// Sources: changelog sync or real-time channel change event.
        /// </summary>
        void OnChannelsUpdated(
            GimGroupChannelCollection collection,
            GimChannelContext context,
            IReadOnlyList<GimGroupChannel> updatedChannels);

        /// <summary>
        /// Channels were removed from the collection.
        /// Sources: changelog sync, real-time delete event, or user left channel.
        /// </summary>
        void OnChannelsDeleted(
            GimGroupChannelCollection collection,
            GimChannelContext context,
            IReadOnlyList<string> deletedChannelUrls);
    }
}
