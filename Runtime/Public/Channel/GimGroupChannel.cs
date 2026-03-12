using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamania.GIMChat.Internal.Domain.UseCases;
using Gamania.GIMChat.Internal.Platform.Unity;
using Logger = Gamania.GIMChat.Internal.Domain.Log.Logger;

namespace Gamania.GIMChat
{
    /// <summary>
    /// Represents a group channel for real-time messaging.
    /// </summary>
    public class GimGroupChannel
    {
        private const string TAG = "GimGroupChannel";

        // ══════════════════════════════════════════════════════════════════════════════
        // Internal Message Sending Events (for SDK internal use only)
        // ══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Internal event fired when a pending message is created locally.
        /// Used by MessageCollection to track pending messages.
        /// </summary>
        internal static event Action<GimGroupChannel, GimBaseMessage> InternalMessagePending;

        /// <summary>
        /// Internal event fired when a message is successfully sent (server ACK).
        /// Used by MessageCollection to update message status.
        /// </summary>
        internal static event Action<GimGroupChannel, GimBaseMessage> InternalMessageSent;

        /// <summary>
        /// Internal event fired when a message fails to send.
        /// Used by MessageCollection to track failed messages.
        /// </summary>
        internal static event Action<GimGroupChannel, GimBaseMessage, GimException> InternalMessageFailed;

        #region Properties

        /// <summary>Unique URL identifier of the channel.</summary>
        public string ChannelUrl { get; set; }

        /// <summary>Display name of the channel.</summary>
        public string Name { get; set; }

        /// <summary>Unix timestamp (milliseconds) when the channel was created.</summary>
        public long CreatedAt { get; set; }

        /// <summary>The most recent message in the channel.</summary>
        public GimBaseMessage LastMessage { get; set; }

        /// <summary>List of users who are members of this channel.</summary>
        public List<GimUser> Members { get; set; }

        /// <summary>Total number of members in the channel.</summary>
        public int MemberCount { get; set; }

        /// <summary>Custom data associated with the channel (JSON string).</summary>
        public string Data { get; set; }

        /// <summary>URL of the channel's cover image.</summary>
        public string CoverUrl { get; set; }

        /// <summary>Custom type for categorizing the channel.</summary>
        public string CustomType { get; set; }

        /// <summary>Whether the channel is distinct.</summary>
        public bool IsDistinct { get; set; }

        /// <summary>Whether the channel is public.</summary>
        public bool IsPublic { get; set; }

        /// <summary>Role of current user in this channel.</summary>
        public GimRole MyRole { get; set; } = GimRole.None;

        #endregion

        #region Send Message

        /// <summary>
        /// Sends a user message to this group channel (callback version).
        /// Returns immediately with a pending message object.
        /// </summary>
        /// <param name="createParams">Parameters for creating the message.</param>
        /// <param name="callback">Callback invoked with the sent message or error.</param>
        /// <returns>Pending message object (status will be updated on completion).</returns>
        public GimUserMessage SendUserMessage(GimUserMessageCreateParams createParams, GimUserMessageHandler callback)
        {
            if (callback == null)
            {
                Logger.Warning(TAG, "SendUserMessage: callback is null");
                return null;
            }

            var pending = CreatePendingUserMessage(createParams);

            // Trigger pending event for MessageCollection
            TriggerMessagePending(this, pending);

            _ = AsyncCallbackHelper.ExecuteAsync(
                () => SendUserMessageCoreAsync(createParams, pending),
                (msg, err) =>
                {
                    // Trigger sent/failed event based on result
                    if (err == null)
                    {
                        TriggerMessageSent(this, msg);
                    }
                    else
                    {
                        TriggerMessageFailed(this, pending, err);
                    }
                    callback(msg, err);
                },
                TAG,
                "SendUserMessage"
            );
            return pending;
        }

        /// <summary>
        /// Sends a user message to this group channel (async version).
        /// If auto-resend is enabled, failed messages due to connection issues
        /// will be automatically queued for resend on reconnection.
        /// </summary>
        /// <param name="createParams">Parameters for creating the user message.</param>
        /// <returns>The sent user message.</returns>
        public async Task<GimUserMessage> SendUserMessageAsync(GimUserMessageCreateParams createParams)
        {
            var pending = CreatePendingUserMessage(createParams);

            // Trigger pending event for MessageCollection
            TriggerMessagePending(this, pending);

            try
            {
                var sent = await SendUserMessageCoreAsync(createParams, pending);
                TriggerMessageSent(this, sent);
                return sent;
            }
            catch (GimException ex)
            {
                TriggerMessageFailed(this, pending, ex);
                throw;
            }
        }

        private GimUserMessage CreatePendingUserMessage(GimUserMessageCreateParams createParams)
        {
            return new GimUserMessage
            {
                ReqId = Guid.NewGuid().ToString("N"),
                ChannelUrl = ChannelUrl,
                Message = createParams?.Message,
                CustomType = createParams?.CustomType,
                Data = createParams?.Data,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                SendingStatus = GimSendingStatus.Pending,
                ErrorCode = null,
                Sender = BuildPendingSender(MyRole)
            };
        }

        private static GimSender BuildPendingSender(GimRole role)
        {
            var currentUser = GIMChat.CurrentUser;
            if (currentUser == null)
            {
                return null;
            }

            return new GimSender
            {
                UserId = currentUser.UserId,
                Nickname = currentUser.Nickname,
                ProfileUrl = currentUser.ProfileUrl,
                Role = role
            };
        }

        private async Task<GimUserMessage> SendUserMessageCoreAsync(GimUserMessageCreateParams createParams, GimUserMessage pending)
        {
            var repository = GIMChatMain.Instance.GetMessageRepository();
            var autoResender = GIMChatMain.Instance.GetMessageAutoResender();
            var useCase = new SendMessageUseCase(repository, autoResender);
            var sent = await useCase.ExecuteAsync(ChannelUrl, createParams, pending);
            return GimUserMessage.FromBase(sent);
        }

        #endregion

        #region Resend Message

        /// <summary>
        /// Resends a failed user message (callback version).
        /// Only works for messages with resendable error codes.
        /// </summary>
        /// <param name="userMessage">The failed message to resend.</param>
        /// <param name="callback">Callback invoked with the resent message or error.</param>
        public void ResendUserMessage(GimUserMessage userMessage, GimUserMessageHandler callback)
        {
            if (callback == null)
            {
                Logger.Warning(TAG, "ResendUserMessage: callback is null");
                return;
            }

            _ = AsyncCallbackHelper.ExecuteAsync(
                () => ResendUserMessageAsync(userMessage),
                (msg, err) => callback(msg, err),
                TAG,
                "ResendUserMessage"
            );
        }

        /// <summary>
        /// Resends a failed user message (async version).
        /// Only works for messages with resendable error codes.
        /// </summary>
        /// <param name="userMessage">The failed message to resend.</param>
        /// <returns>The resent user message.</returns>
        /// <exception cref="GimException">Thrown if message is not resendable.</exception>
        public async Task<GimUserMessage> ResendUserMessageAsync(GimUserMessage userMessage)
        {
            ValidateResendable(userMessage);

            var createParams = new GimUserMessageCreateParams
            {
                Message = userMessage.Message,
                CustomType = userMessage.CustomType,
                Data = userMessage.Data
            };

            var sent = await SendUserMessageAsync(createParams);
            return GimUserMessage.FromBase(sent);
        }

        private void ValidateResendable(GimUserMessage userMessage)
        {
            if (userMessage == null)
                throw new GimException(GimErrorCode.InvalidParameter, "userMessage is null");

            if (string.IsNullOrEmpty(userMessage.ChannelUrl) || userMessage.ChannelUrl != ChannelUrl)
                throw new GimException(GimErrorCode.InvalidParameter, "message channel mismatch");

            if (userMessage.SendingStatus != GimSendingStatus.Failed)
                throw new GimException(GimErrorCode.InvalidParameter, "message is not in Failed state");

            if (!userMessage.ErrorCode.HasValue || !userMessage.ErrorCode.Value.IsResendable())
                throw new GimException(GimErrorCode.InvalidParameter, "message is not resendable");
        }

        #endregion

        #region Channel Event Handlers

        private static readonly Dictionary<string, GimGroupChannelHandler> _handlers = new();

        /// <summary>
        /// Adds a group channel handler to receive message events.
        /// </summary>
        /// <param name="handlerId">Unique identifier for this handler.</param>
        /// <param name="handler">Handler containing callback functions.</param>
        public static void AddGroupChannelHandler(string handlerId, GimGroupChannelHandler handler)
        {
            if (_handlers.ContainsKey(handlerId))
            {
                Logger.Warning(TAG, $"Handler already exists: {handlerId}");
                return;
            }

            _handlers[handlerId] = handler;
            Logger.Debug(TAG, $"Added group channel handler: {handlerId}");
        }

        /// <summary>
        /// Returns the handler registered with the given id, or null if not found.
        /// </summary>
        public static GimGroupChannelHandler GetGroupChannelHandler(string handlerId)
        {
            return _handlers.TryGetValue(handlerId, out var handler) ? handler : null;
        }

        /// <summary>
        /// Removes a group channel handler.
        /// </summary>
        /// <param name="handlerId">Unique identifier of the handler to remove.</param>
        public static void RemoveGroupChannelHandler(string handlerId)
        {
            if (_handlers.Remove(handlerId))
            {
                Logger.Debug(TAG, $"Removed group channel handler: {handlerId}");
            }
            else
            {
                Logger.Warning(TAG, $"Handler not found: {handlerId}");
            }
        }

        /// <summary>
        /// Remove all registered group channel handlers.
        /// Called during disconnect/logout to clean up all listeners.
        /// </summary>
        public static void RemoveAllGroupChannelHandlers()
        {
            var count = _handlers.Count;
            _handlers.Clear();
            Logger.Debug(TAG, $"Removed all group channel handlers (count: {count})");
        }

        /// <summary>
        /// Triggers message received event for all registered handlers.
        /// Called internally when a new message is received via WebSocket.
        /// </summary>
        internal static void TriggerMessageReceived(GimGroupChannel channel, GimBaseMessage message)
        {
            foreach (var handler in _handlers.Values)
            {
                try
                {
                    handler.OnMessageReceived?.Invoke(channel, message);
                }
                catch (Exception e)
                {
                    Logger.Error(TAG, "Error in OnMessageReceived handler", e);
                }
            }
        }

        /// <summary>
        /// Triggers message updated event for all registered handlers.
        /// Called internally when a message is updated via WebSocket.
        /// </summary>
        internal static void TriggerMessageUpdated(GimGroupChannel channel, GimBaseMessage message)
        {
            foreach (var handler in _handlers.Values)
            {
                try
                {
                    handler.OnMessageUpdated?.Invoke(channel, message);
                }
                catch (Exception e)
                {
                    Logger.Error(TAG, "Error in OnMessageUpdated handler", e);
                }
            }
        }

        /// <summary>
        /// Triggers channel changed event for all registered handlers.
        /// Call when WebSocket receives channel metadata update.
        /// </summary>
        internal static void TriggerChannelChanged(GimGroupChannel channel)
        {
            foreach (var handler in _handlers.Values)
            {
                try
                {
                    handler.OnChannelChanged?.Invoke(channel);
                }
                catch (Exception e)
                {
                    Logger.Error(TAG, "Error in OnChannelChanged handler", e);
                }
            }
        }

        /// <summary>
        /// Triggers channel deleted event for all registered handlers.
        /// Call when WebSocket receives channel delete event.
        /// </summary>
        internal static void TriggerChannelDeleted(string channelUrl)
        {
            foreach (var handler in _handlers.Values)
            {
                try
                {
                    handler.OnChannelDeleted?.Invoke(channelUrl);
                }
                catch (Exception e)
                {
                    Logger.Error(TAG, "Error in OnChannelDeleted handler", e);
                }
            }
        }

        /// <summary>
        /// Triggers message pending event via internal static event.
        /// Called internally when a pending message is created locally.
        /// </summary>
        internal static void TriggerMessagePending(GimGroupChannel channel, GimBaseMessage message)
        {
            try
            {
                InternalMessagePending?.Invoke(channel, message);
            }
            catch (Exception e)
            {
                Logger.Error(TAG, "Error in InternalMessagePending event", e);
            }
        }

        /// <summary>
        /// Triggers message sent event via internal static event.
        /// Called internally when a pending message is successfully sent (server ACK).
        /// </summary>
        internal static void TriggerMessageSent(GimGroupChannel channel, GimBaseMessage message)
        {
            try
            {
                InternalMessageSent?.Invoke(channel, message);
            }
            catch (Exception e)
            {
                Logger.Error(TAG, "Error in InternalMessageSent event", e);
            }
        }

        /// <summary>
        /// Triggers message failed event via internal static event.
        /// Called internally when a message fails to send.
        /// </summary>
        internal static void TriggerMessageFailed(GimGroupChannel channel, GimBaseMessage message, GimException error)
        {
            try
            {
                InternalMessageFailed?.Invoke(channel, message, error);
            }
            catch (Exception e)
            {
                Logger.Error(TAG, "Error in InternalMessageFailed event", e);
            }
        }

        #endregion
    }
}
