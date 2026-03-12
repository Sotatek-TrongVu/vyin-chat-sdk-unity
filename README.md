# Vyin Chat Unity SDK

Real-time messaging SDK for Unity.

## Requirements

- Unity 2022.3 or newer

## Installation

Add Vyin Chat SDK via Unity Package Manager:

1. **Window** > **Package Manager** > **+** > **Add package from git URL**
2. Enter:
   ```
   https://github.com/vyinchat/vyin-chat-sdk-unity.git
   ```

## Quick Start

Add the namespace to your script:

```csharp
using Gamania.GIMChat;
```

### 1. Initialize SDK

```csharp
GIMChat.Init(new GimInitParams("YOUR_APP_ID"));
```

### 2. Connect User

```csharp
GIMChat.Connect(userId, authToken, (user, error) =>
{
    if (error != null)
    {
        // Handle connect error
        return;
    }
    Debug.Log($"Connected as {user.UserId}");
});
```

### 3. Create a Group Channel

```csharp
var channelParams = new GimGroupChannelCreateParams
{
    Name = "general-room",
    UserIds = new List<string> { userId },
    IsDistinct = true
};

GimGroupChannelModule.CreateGroupChannel(channelParams, (channel, error) =>
{
    if (error != null)
    {
        // Handle create channel error
        return;
    }

    _currentChannel = channel;
    Debug.Log($"Channel ready: {channel.ChannelUrl}");
});
```

### 4. Send a Message

```csharp
var messageParams = new GimUserMessageCreateParams
{
    Message = "Hello everyone!"
};

GimUserMessage pending = _currentChannel.SendUserMessage(messageParams, (message, error) =>
{
    if (error != null)
    {
        // Handle send error
        return;
    }

    Debug.Log($"Message sent: id={message.MessageId}, status={message.SendingStatus}");
});

Debug.Log($"Pending message created: reqId={pending?.ReqId}, status={pending?.SendingStatus}");
```

### 5. Receive Messages in Real-time

**Option A: Channel Handler (simple callback)**

```csharp
var handler = new GimGroupChannelHandler
{
    OnMessageReceived = (channel, message) =>
    {
        Debug.Log($"New message: {message.Message}");
    },
    OnMessageUpdated = (channel, message) =>
    {
        Debug.Log($"Message updated: {message.Message}");
    }
};

GimGroupChannel.AddGroupChannelHandler("my-handler", handler);

// Remove when no longer needed
void OnDestroy()
{
    GimGroupChannel.RemoveGroupChannelHandler("my-handler");
}
```

**Option B: MessageCollection (recommended — handles history + real-time)**

```csharp
var collection = GIMChat.CreateMessageCollection(channel, pageLimit: 20);
collection.Delegate = this; // implement IGimMessageCollectionDelegate

// Load initial messages
var messages = await collection.LoadMoreAsync();

// IGimMessageCollectionDelegate callbacks
public void OnMessagesAdded(GimMessageCollection collection, IReadOnlyList<GimMessage> messages) { }
public void OnMessagesUpdated(GimMessageCollection collection, IReadOnlyList<GimMessage> messages) { }
public void OnMessagesDeleted(GimMessageCollection collection, IReadOnlyList<GimMessage> messages) { }

// Dispose when done
collection.Dispose();
```

## License

This SDK is provided under the Vyin Chat SDK License Agreement.
See [LICENSE.md](./LICENSE.md) for details.
