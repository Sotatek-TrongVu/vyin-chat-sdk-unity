using System;

namespace Gamania.GIMChat
{
    /// <summary>
    /// Handler for receiving group channel events such as new messages and message updates.
    /// Extended for GroupChannelCollection: OnChannelChanged, OnChannelDeleted.
    /// </summary>
    public class GimGroupChannelHandler
    {
        /// <summary>
        /// Invoked when a new message is received in the channel
        /// </summary>
        public Action<GimGroupChannel, GimBaseMessage> OnMessageReceived;

        /// <summary>
        /// Invoked when an existing message is updated (e.g., streaming AI responses)
        /// </summary>
        public Action<GimGroupChannel, GimBaseMessage> OnMessageUpdated;

        /// <summary>
        /// Invoked when a message is deleted from the channel.
        /// </summary>
        public Action<GimGroupChannel, long> OnMessageDeleted;

        /// <summary>
        /// Invoked when channel metadata changes (e.g., name, cover).
        /// Used by GroupChannelCollection for OnChannelsUpdated.
        /// </summary>
        public Action<GimGroupChannel> OnChannelChanged;

        /// <summary>
        /// Invoked when a channel is deleted.
        /// Used by GroupChannelCollection for OnChannelsDeleted.
        /// </summary>
        public Action<string> OnChannelDeleted;

    }
}