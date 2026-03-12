using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gamania.GIMChat;
using System;
using System.Collections.Generic;

/// <summary>
/// Vyin Chat SDK Sample - Demonstrates core SDK functionality.
///
/// SDK Usage Flow:
/// 1. Initialize SDK with GIMChat.Init()
/// 2. Connect to server with GIMChat.Connect()
/// 3. Create or get a channel with GimGroupChannelModule
/// 4. Send messages with GimGroupChannel.SendUserMessage()
/// 5. Receive real-time updates via GimGroupChannelCollection / GimMessageCollection
/// </summary>
public class VyinChatSampleController : MonoBehaviour, IGimSessionHandler, IGimGroupChannelCollectionDelegate, IGimMessageCollectionDelegate
{
    #region Inspector Fields

    [Header("SDK Configuration")]
    [Tooltip("App ID from the Vyin Chat console (default: PROD)")]
    [SerializeField] private string appId = "adb53e88-4c35-469a-a888-9e49ef1641b2";

    [Tooltip("User ID for testing")]
    [SerializeField] private string userId = "testuser1";

    [Tooltip("Auth Token (optional, leave empty if not needed)")]
    [SerializeField] private string authToken = "";

    [Header("Channel Configuration")]
    [Tooltip("Channel name to create")]
    [SerializeField] private string channelName = "Unity Test Channel";

    [Tooltip("Bot ID to invite to the channel")]
    [SerializeField] private string botId = "vyin_chat_openai";

    [Tooltip("Other users to invite to the channel (optional)")]
    [SerializeField] private List<string> inviteUserIds = new();

    [Header("Collection Configuration")]
    [Tooltip("Number of channels to load per page in collection")]
    [SerializeField] private int pageLimit = 10;

    [Header("Debug")]
    [Tooltip("Enable automatic message resend on reconnection")]
    [SerializeField] private bool enableAutoResend = true;

    [Tooltip("When enabled, shows each sending status (Pending → Succeeded/Failed) as separate entries")]
    [SerializeField] private bool showSendingStatusTransitions;

    [Tooltip("When enabled, updated messages are shown as separate entries instead of replacing the original")]
    [SerializeField] private bool showUpdatesAsSeparateMessages;

    [Tooltip("New token to provide when SDK requests refresh")]
    [SerializeField] private string refreshToken = "demo-refresh-token";

    [Tooltip("Simulate token refresh failure")]
    [SerializeField] private bool simulateRefreshFailure;

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button loadMoreButton;
    [SerializeField] private Button loadPreviousMessagesButton;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TextMeshProUGUI connectionStatusText;

    #endregion

    #region Private Fields

    private const string HANDLER_ID = "VyinChatSampleHandler";
    private GimGroupChannel _currentChannel;
    private readonly Dictionary<string, (string message, string meta1, string meta2)> _messageCache = new();
    private GimGroupChannelCollection _channelCollection;
    private GimMessageCollection _messageCollection;

    // Token refresh state
    private Action<string> _tokenRefreshSuccess;
    private Action _tokenRefreshFail;
    private bool _isWaitingForToken;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        SetupUI();
        InitializeAndConnect();
    }

    private void OnDestroy()
    {
        // Cleanup handlers
        GimGroupChannel.RemoveGroupChannelHandler(HANDLER_ID);
        GIMChat.RemoveConnectionHandler(HANDLER_ID);
        _channelCollection?.Dispose();
        _messageCollection?.Dispose();
    }

    #endregion

    #region Sample Step 1: Initialize

    /// <summary>
    /// Step 1: Initialize SDK with app configuration.
    /// </summary>
    private void InitializeAndConnect()
    {
        LogInfo($"AppId: {appId}");

        // Step 1: Initialize SDK
        var initParams = new GimInitParams(appId, logLevel: GimLogLevel.Debug);
        GIMChat.Init(initParams);
        LogInfo("SDK Initialized");

        // Enable auto-resend for message reliability testing
        GIMChat.SetEnableMessageAutoResend(enableAutoResend);
        LogInfo($"Auto-resend: {(enableAutoResend ? "enabled" : "disabled")}");

        // Set session handler for token refresh
        GIMChat.SetSessionHandler(this);
        LogInfo("Session handler registered");

        // Register connection handler
        RegisterConnectionHandler();

        Connect();
    }

    #endregion

    #region Sample Step 2: Connect

    /// <summary>
    /// Step 2: Connect to Vyin Chat with user credentials.
    /// </summary>
    private void Connect()
    {
        var token = string.IsNullOrEmpty(authToken) ? null : authToken;

        LogInfo($"Connecting as '{userId}'...");
        UpdateConnectionStatusDisplay("Connecting...", ConnectingColor);
        GIMChat.Connect(userId, token, OnConnected);
    }

    private void OnConnected(GimUser user, GimException error)
    {
        if (error != null)
        {
            LogError($"Connection failed: {error.Message}");
            return;
        }

        LogInfo($"Connected! Welcome, {user.UserId}!");

        // Initialize Collection
        InitChannelCollectionAndLoad();

        // Proceed to Step 3
        CreateOrGetChannel();
    }

    #endregion

    #region Sample Step 3: Create or Get Channel

    /// <summary>
    /// Step 3: Create or get a group channel.
    /// </summary>
    private void CreateOrGetChannel()
    {
        LogSeparator();
        LogInfo($"Creating channel '{channelName}'...");

        // Build member list
        var members = new List<string> { userId };
        if (!string.IsNullOrEmpty(botId))
        {
            LogInfo($"Creating with botId: '{botId}'...");
            members.Add(botId);
        }
        members.AddRange(inviteUserIds);

        // Create channel params
        var createParams = new GimGroupChannelCreateParams
        {
            Name = channelName,
            UserIds = members,
            OperatorUserIds = new List<string> { userId },
            IsDistinct = true  // Reuse existing channel if members match
        };

        // Create channel
        GimGroupChannelModule.CreateGroupChannel(createParams, OnChannelCreated);
    }

    private void OnChannelCreated(GimGroupChannel channel, GimException error)
    {
        if (error != null)
        {
            LogError($"Failed to create channel: {error.Message}");
            return;
        }

        LogInfo($"Channel created!");

        // Get channel info
        GimGroupChannelModule.GetGroupChannel(channel.ChannelUrl, OnChannelRetrieved);
    }

    private void OnChannelRetrieved(GimGroupChannel channel, GimException error)
    {
        if (error != null)
        {
            LogError($"Failed to get channel: {error.Message}");
            return;
        }

        _currentChannel = channel;
        LogInfo($"Channel ready!");
        LogInfo($"  Name: {channel.Name}");
        LogInfo($"  URL: {channel.ChannelUrl}");

        // Step 5: Start MessageCollection for real-time updates
        InitMessageCollectionAndLoad();
        // RegisterGroupChannelMessageHandler();

        LogSeparator();
        LogInfo("Ready to chat! Type a message above.");
    }

    #endregion

    #region Sample Step 4: Send Message

    /// <summary>
    /// Step 4: Send a user message.
    /// </summary>
    private void SendChatMessage(string text)
    {
        if (_currentChannel == null)
        {
            LogError("No channel available");
            return;
        }

        // Send message via SDK
        var messageParams = new GimUserMessageCreateParams { Message = text };
        GimUserMessage pending = null;
        pending = _currentChannel.SendUserMessage(messageParams, (msg, err) => OnMessageSent(msg, err, pending?.ReqId, text));
        if (pending != null)
        {
            ShowPendingMessage(pending.ReqId, text);
        }
    }

    private void OnMessageSent(GimUserMessage message, GimException error, string pendingId, string originalText)
    {
        if (error != null)
        {
            LogError($"Failed to send: {error.Message}");
            if (!string.IsNullOrEmpty(pendingId))
            {
                var failedId = showSendingStatusTransitions ? $"{pendingId}-failed" : pendingId;
                UpdateMessageDisplay(failedId, $"[{userId}] {originalText}", $"  -> {GimSendingStatus.Failed}");
            }
            return;
        }

        if (message != null)
        {
            var displayName = GetDisplayName(message);
            if (!showSendingStatusTransitions && !string.IsNullOrEmpty(pendingId))
            {
                _messageCache.Remove(pendingId);
            }
            UpdateMessageDisplay(message.MessageId.ToString(), $"[{displayName}] {message.Message}", $"  -> {message.SendingStatus}");
        }
    }

    #endregion

    #region Sample Step 5: Collection (Real-time Updates)

    private async void InitChannelCollectionAndLoad()
    {
        LogSeparator();
        LogInfo($"Creating GroupChannelCollection (limit={pageLimit})...");

        _channelCollection = GIMChat.CreateGroupChannelCollection(pageLimit);
        _channelCollection.Delegate = this;

        LogInfo("Loading channels via Collection...");
        try
        {
            var channels = await _channelCollection.LoadMoreAsync();
            LogInfo($"Collection loaded {channels.Count} channels.");
            // NOTE: Changelog is automatically triggered after first LoadMore
            // No need to manually call RequestChangeLogsAsync() here
        }
        catch (GimException ex)
        {
            LogError($"Collection Load/Sync failed: {ex.Message}");
        }
    }

    private void InitMessageCollectionAndLoad()
    {
        LogSeparator();
        LogInfo($"Creating MessageCollection for {_currentChannel.ChannelUrl}...");

        _messageCollection = GIMChat.CreateMessageCollection(_currentChannel);
        _messageCollection.Delegate = this;

        LogInfo("Loading initial messages via MessageCollection...");
        _messageCollection.StartCollection((messages, error) =>
        {
            if (error != null)
            {
                LogError($"MessageCollection Start failed: {error.Message}");
                return;
            }

            LogInfo($"MessageCollection loaded {messages.Count} initial messages.");
            foreach (var msg in messages)
            {
                var displayName = GetDisplayName(msg);
                UpdateMessageDisplay(msg.MessageId.ToString(), $"[{displayName}] {msg.Message}");
            }
            LogInfo($"  HasPrevious: {_messageCollection.HasPrevious}");
            LogInfo($"  HasNext: {_messageCollection.HasNext}");
        });
    }

    /// <summary>
    /// [TEST] Manually trigger LoadMore for testing pagination.
    /// Right-click on VyinChatSampleController component in Inspector → Test LoadMore
    /// </summary>
    [ContextMenu("Test LoadMore Channels")]
    private async void TestLoadMoreChannels()
    {
        if (_channelCollection == null)
        {
            LogError("[TestLoadMore] Collection not created yet!");
            return;
        }

        LogSeparator();
        LogInfo($"[TestLoadMore] Calling LoadMoreAsync()...");
        LogInfo($"  Before: {_channelCollection.ChannelList.Count} channels in collection");
        LogInfo($"  HasNext: {_channelCollection.HasNext}");
        LogInfo($"  IsLoading: {_channelCollection.IsLoading}");

        if (!_channelCollection.HasNext)
        {
            LogInfo("[TestLoadMore] No more channels to load!");
            return;
        }

        if (_channelCollection.IsLoading)
        {
            LogInfo("[TestLoadMore] Already loading, please wait...");
            return;
        }

        try
        {
            var channels = await _channelCollection.LoadMoreAsync();
            LogInfo($"[TestLoadMore] Loaded {channels.Count} new channel(s):");

            // Display detailed info for each newly loaded channel
            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];
                var lastMsg = ch.LastMessage?.Message ?? "(no messages)";
                var createdAt = DateTimeOffset.FromUnixTimeMilliseconds(ch.CreatedAt).ToString("yyyy-MM-dd HH:mm");

                LogInfo($"  [{i + 1}] {ch.Name ?? "(unnamed)"}");
                LogInfo($"      URL: {ch.ChannelUrl}");
                LogInfo($"      Members: {ch.MemberCount} | Created: {createdAt}");
                LogInfo($"      Last Message: {lastMsg}");
            }

            LogInfo($"[TestLoadMore] Total channels now: {_channelCollection.ChannelList.Count}");
            LogInfo($"[TestLoadMore] HasNext: {_channelCollection.HasNext}");

            if (_channelCollection.HasNext)
            {
                LogInfo("[TestLoadMore] Right-click again to load more!");
            }
            else
            {
                LogInfo("[TestLoadMore] All channels loaded!");
            }

            LogSeparator();
        }
        catch (GimException ex)
        {
            LogError($"[TestLoadMore] Failed: {ex.Message}");
        }
    }

    [ContextMenu("Test Load Previous Messages")]
    private async void TestLoadPreviousMessages()
    {
        if (_messageCollection == null)
        {
            LogError("[TestLoadPrevious] MessageCollection not created yet!");
            return;
        }

        LogSeparator();
        LogInfo($"[TestLoadPrevious] Calling LoadPreviousAsync()...");

        if (!_messageCollection.HasPrevious)
        {
            LogInfo("[TestLoadPrevious] No more previous messages to load!");
            return;
        }

        try
        {
            var messages = await _messageCollection.LoadPreviousAsync();
            LogInfo($"[TestLoadPrevious] Loaded {messages.Count} previous message(s).");
            
            // NOTE: We could display them here, but the sample UI just appends to the bottom
            // So we'll just log them to the console for verification.
            foreach (var msg in messages)
            {
                 LogInfo($"  <- [{GetDisplayName(msg)}] {msg.Message}");
            }
            LogInfo($"  HasPrevious: {_messageCollection.HasPrevious}");
        }
        catch (GimException ex)
        {
            LogError($"[TestLoadPrevious] Failed: {ex.Message}");
        }
    }

    #endregion

    #region IGimGroupChannelCollectionDelegate Implementation

    public void OnChannelsAdded(GimGroupChannelCollection collection, GimChannelContext context, IReadOnlyList<GimGroupChannel> channels)
    {
        LogInfo($"[Channel Collection] OnChannelsAdded: {channels.Count} channel(s), Source: {context.Source}");
        foreach (var ch in channels)
        {
            LogInfo($"  + {ch.Name ?? ch.ChannelUrl}");
        }
    }

    public void OnChannelsUpdated(GimGroupChannelCollection collection, GimChannelContext context, IReadOnlyList<GimGroupChannel> channels)
    {
        LogInfo($"[Channel Collection] OnChannelsUpdated: {channels.Count} channel(s), Source: {context.Source}");
        foreach (var ch in channels)
        {
            LogInfo($"  [updated] {ch.Name ?? ch.ChannelUrl}");
        }
    }

    public void OnChannelsDeleted(GimGroupChannelCollection collection, GimChannelContext context, IReadOnlyList<string> channelUrls)
    {
        LogInfo($"[Channel Collection] OnChannelsDeleted: {channelUrls.Count} channel(s), Source: {context.Source}");
        foreach (var url in channelUrls)
        {
            LogInfo($"  - {url}");
        }
    }

    #endregion

    #region IGimMessageCollectionDelegate Implementation

    public void OnMessagesAdded(GimMessageCollection collection, GimMessageContext context, GimGroupChannel channel, IReadOnlyList<GimBaseMessage> addedMessages)
    {
        LogInfo($"[Message Collection] OnMessagesAdded: {addedMessages.Count} message(s), Source: {context.Source}");
        foreach (var message in addedMessages)
        {
            if (message.Sender?.UserId == userId) continue; // Skip own messages if we already showed them via sent callback
            
            var displayName = GetDisplayName(message);
            var meta1 = $"  -> Received | Type: '{message.CustomType}' | Done: {message.Done}";
            UpdateMessageDisplay(message.MessageId.ToString(), $"[{displayName}] {message.Message}", meta1);
        }
    }

    public void OnMessagesUpdated(GimMessageCollection collection, GimMessageContext context, GimGroupChannel channel, IReadOnlyList<GimBaseMessage> updatedMessages)
    {
        LogInfo($"[Message Collection] OnMessagesUpdated: {updatedMessages.Count} message(s), Source: {context.Source}");
        foreach (var message in updatedMessages)
        {
            if (message.Sender?.UserId == userId) continue;
            
            var displayName = GetDisplayName(message);
            var meta1 = $"  -> Updated | Type: '{message.CustomType}' | Done: {message.Done}";
            
            var displayId = (showUpdatesAsSeparateMessages)
                ? $"{message.MessageId}-{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
                : message.MessageId.ToString();

            UpdateMessageDisplay(displayId, $"[{displayName}] {message.Message}", meta1);
        }
    }

    public void OnMessagesDeleted(GimMessageCollection collection, GimMessageContext context, GimGroupChannel channel, IReadOnlyList<GimBaseMessage> deletedMessages)
    {
        LogInfo($"[Message Collection] OnMessagesDeleted: {deletedMessages.Count} message(s), Source: {context.Source}");
        foreach (var message in deletedMessages)
        {
            UpdateMessageDisplay($"{message.MessageId}-deleted", $"[Deleted] Message {message.MessageId} was deleted.", null);
        }
    }

    public void OnChannelUpdated(GimMessageCollection collection, GimMessageContext context, GimGroupChannel channel)
    {
        LogInfo($"[Message Collection] OnChannelUpdated: {channel.Name}, Source: {context.Source}");
    }

    public void OnChannelDeleted(GimMessageCollection collection, GimMessageContext context, string channelUrl)
    {
        LogInfo($"[Message Collection] OnChannelDeleted: {channelUrl}, Source: {context.Source}");
    }

    public void OnHugeGapDetected(GimMessageCollection collection)
    {
        LogInfo($"[Message Collection] Huge Gap Detected! Re-initializing collection...");
        // Handle huge gap by re-starting the collection
        InitMessageCollectionAndLoad();
    }

    #endregion

    #region UI Helpers

    private void SetupUI()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }

        if (loadMoreButton != null)
        {
            loadMoreButton.onClick.AddListener(TestLoadMoreChannels);
        }
        
        if (loadPreviousMessagesButton != null)
        {
            loadPreviousMessagesButton.onClick.AddListener(TestLoadPreviousMessages);
        }

        UpdateConnectionStatusDisplay("Disconnected", DisconnectedColor);
    }

    private static readonly Color ConnectedColor = new(0.2f, 0.8f, 0.4f); // Soft green
    private static readonly Color DisconnectedColor = new(0.9f, 0.3f, 0.3f); // Soft red
    private static readonly Color ConnectingColor = new(0.3f, 0.6f, 0.9f); // Soft blue
    private static readonly Color ReconnectingColor = new(1f, 0.7f, 0.2f); // Amber

    private void RegisterConnectionHandler()
    {
        var handler = new GimConnectionHandler
        {
            OnConnected = _ => UpdateConnectionStatusDisplay("Connected", ConnectedColor),
            OnDisconnected = _ => UpdateConnectionStatusDisplay("Disconnected", DisconnectedColor),
            OnReconnectStarted = () => UpdateConnectionStatusDisplay("Reconnecting...", ReconnectingColor),
            OnReconnectSucceeded = () => UpdateConnectionStatusDisplay("Connected", ConnectedColor),
            OnReconnectFailed = () => UpdateConnectionStatusDisplay("Reconnect Failed", DisconnectedColor)
        };
        GIMChat.AddConnectionHandler(HANDLER_ID, handler);
    }

    private void UpdateConnectionStatusDisplay(string text, Color color)
    {
        if (connectionStatusText == null) return;
        connectionStatusText.text = text;
        connectionStatusText.color = color;
    }

    private void OnSendButtonClicked()
    {
        if (inputField == null) return;

        var text = inputField.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        inputField.text = "";
        inputField.ActivateInputField();

        if (_currentChannel == null)
        {
            LogInfo("No channel available. Creating one...");
            CreateOrGetChannel();
            return;
        }

        SendChatMessage(text);
    }

    private void ShowPendingMessage(string pendingId, string text)
    {
        UpdateMessageDisplay(pendingId, $"[{userId}] {text}", $"  -> {GimSendingStatus.Pending}");
    }

    private static string GetDisplayName(GimBaseMessage message)
    {
        if (message?.Sender == null) return "Unknown";
        return string.IsNullOrEmpty(message.Sender.Nickname)
            ? message.Sender.UserId
            : message.Sender.Nickname;
    }

    #endregion

    #region Logging

    protected void LogInfo(string message)
    {
        var formatted = $"[Sample] {message}";
        Debug.Log(formatted);
        AppendToLog(formatted);
    }

    protected void LogError(string message)
    {
        var formatted = $"[Sample] ERROR: {message}";
        Debug.LogError(formatted);
        AppendToLog(formatted);
    }

    protected void LogSeparator()
    {
        AppendToLog("────────────────────────────────");
    }

    private void AppendToLog(string text)
    {
        if (logText != null)
        {
            logText.text += text + "\n";
            ScrollToBottom();
        }
    }

    private void UpdateMessageDisplay(string messageId, string message, string meta1 = null, string meta2 = null)
    {
        _messageCache[messageId] = (message, meta1, meta2);

        if (logText == null) return;

        var lines = new List<string>();
        foreach (var entry in _messageCache.Values)
        {
            lines.Add(entry.message);
            if (!string.IsNullOrEmpty(entry.meta1))
                lines.Add(entry.meta1);
            if (!string.IsNullOrEmpty(entry.meta2))
                lines.Add(entry.meta2);
        }

        logText.text = string.Join("\n", lines);
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            StartCoroutine(ScrollToBottomCoroutine());
        }
    }

    private System.Collections.IEnumerator ScrollToBottomCoroutine()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    #endregion

    #region IGimSessionHandler Implementation

    /// <summary>
    /// Called when SDK needs a new token.
    /// In production, fetch from your auth server.
    /// </summary>
    public void OnSessionTokenRequired(Action<string> success, Action fail)
    {
        _tokenRefreshSuccess = success;
        _tokenRefreshFail = fail;
        _isWaitingForToken = true;

        LogInfo("🔑 SDK requests new token!");
        LogInfo("   Use Inspector to provide token or simulate failure.");

        // Auto-provide token if not simulating failure
        if (!simulateRefreshFailure && !string.IsNullOrEmpty(refreshToken))
        {
            LogInfo($"   Auto-providing token: {refreshToken.Substring(0, System.Math.Min(20, refreshToken.Length))}...");
            ProvideRefreshToken();
        }
    }

    /// <summary>
    /// Called when token refresh succeeded.
    /// </summary>
    public void OnSessionRefreshed()
    {
        LogInfo("✅ Session refreshed successfully!");
        _isWaitingForToken = false;
    }

    /// <summary>
    /// Called when session cannot be recovered.
    /// In production, navigate to login screen.
    /// </summary>
    public void OnSessionClosed()
    {
        LogError("❌ Session closed - would navigate to login");
        _isWaitingForToken = false;
    }

    /// <summary>
    /// Called when an error occurred during refresh.
    /// </summary>
    public void OnSessionError(GimException error)
    {
        LogError($"⚠️ Session error: {error.ErrorCode} - {error.Message}");
        _isWaitingForToken = false;
    }

    /// <summary>
    /// Provide token to SDK (call from Inspector button or code)
    /// </summary>
    public void ProvideRefreshToken()
    {
        if (!_isWaitingForToken)
        {
            LogInfo("Not waiting for token");
            return;
        }

        LogInfo($"Providing token to SDK...");
        _isWaitingForToken = false;
        _tokenRefreshSuccess?.Invoke(refreshToken);
    }

    /// <summary>
    /// Fail token refresh (call from Inspector button or code)
    /// </summary>
    public void FailTokenRefresh()
    {
        if (!_isWaitingForToken)
        {
            LogInfo("Not waiting for token");
            return;
        }

        LogInfo("Failing token refresh...");
        _isWaitingForToken = false;
        _tokenRefreshFail?.Invoke();
    }

    #endregion

}
