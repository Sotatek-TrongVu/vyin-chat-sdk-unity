# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-03-12

Initial public release of Vyin Chat Unity SDK.

### Features

- **Initialization** — Configure the SDK with your App ID via `GIMChat.Init()`
- **User connection** — Connect and authenticate users with `GIMChat.Connect()`, with session token refresh support via `IGimSessionHandler`
- **Group channels** — Create and retrieve distinct group channels via `GimGroupChannelModule`
- **Send messages** — Send user messages with immediate pending state and delivery confirmation via `GimGroupChannel.SendUserMessage()`
- **Real-time messaging** — Receive incoming messages via `GimGroupChannelHandler` callbacks or the recommended `GimMessageCollection` which handles both message history and real-time updates
- **Connection management** — Automatic reconnection with exponential backoff, connection state tracking via `GimConnectionState`, and background disconnection support
- **Message reliability** — Sending status tracking (`Pending`, `Succeeded`, `Failed`) and optional auto-resend on reconnection
- **Async/await support** — All APIs available in both callback and async/await variants
