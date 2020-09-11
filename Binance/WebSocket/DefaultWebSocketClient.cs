﻿using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Binance.WebSocket
{
    /// <summary>
    /// The default <see cref="IWebSocketClient"/> implementation.
    /// Alternative implementations exist for WebSocket4Net and WebSocketSharp.
    /// </summary>
    public sealed class DefaultWebSocketClient : WebSocketClient
    {
        #region Private Constants

        private const int ReceiveBufferSize = 16 * 1024;

        #endregion Private Constants

        #region Private Fields

        private readonly IClientWebSocketFactory _clientWebSocketFactory;

        private readonly object _sync = new object();

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="clientWebSocketFactory"></param>
        /// <param name="logger"></param>
        public DefaultWebSocketClient(IClientWebSocketFactory clientWebSocketFactory = null, ILogger<DefaultWebSocketClient> logger = null)
            : base(logger)
        {
            _clientWebSocketFactory = clientWebSocketFactory ?? new ClientWebSocketFactory();
        }

        #endregion Constructors

        #region Public Methods

        public override async Task StreamAsync(Uri uri, CancellationToken token)
        {
            Throw.IfNull(uri, nameof(uri));

            if (!token.CanBeCanceled)
                throw new ArgumentException($"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: Token must be capable of being in the canceled state.", nameof(token));

            if (token.IsCancellationRequested)
                return;

            lock (_sync)
            {
                if (IsStreaming)
                    throw new InvalidOperationException($"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: Already streaming (this method is not reentrant).");

                IsStreaming = true;
            }

            var webSocket = _clientWebSocketFactory.CreateClientWebSocket();

            try
            {
                try
                {
                    Logger?.LogInformation($"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: Web socket connecting...");

                    await webSocket.ConnectAsync(uri, token)
                        .ConfigureAwait(false);

                    if (webSocket.State != WebSocketState.Open)
                        throw new Exception($"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: WebSocket connect failed (state: {webSocket.State}).");

                    OnOpen();
                }
                catch (OperationCanceledException) { /* ignore */ }
                catch (Exception e)
                {
                    if (!token.IsCancellationRequested)
                    {
                        Logger?.LogWarning(e, $"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: WebSocket connect exception (state: {webSocket.State}).");
                        throw;
                    }
                }

                var bytes = new byte[ReceiveBufferSize];
                var buffer = new ArraySegment<byte>(bytes);

                var stringBuilder = new StringBuilder();

                while (!token.IsCancellationRequested)
                {
                    stringBuilder.Clear();

                    try
                    {
                        WebSocketReceiveResult result;
                        do
                        {
                            if (webSocket.State != WebSocketState.Open)
                                break;

                            result = await webSocket
                                .ReceiveAsync(buffer, token)
                                .ConfigureAwait(false);

                            switch (result.MessageType)
                            {
                                case WebSocketMessageType.Close:
                                    var message = result.CloseStatus.HasValue
                                        ? $"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: Web socket closed (state: {webSocket.State}) (status: {result.CloseStatus.Value}): \"{result.CloseStatusDescription ?? "[no reason provided]"}\""
                                        : $"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: Web socket closed (state: {webSocket.State}): \"{result.CloseStatusDescription ?? "[no reason provided]"}\"";

                                    Logger?.LogWarning(message);

                                    try
                                    {
                                        await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None)
                                            .ConfigureAwait(false);
                                    }
                                    catch (Exception e)
                                    {
                                        Logger?.LogWarning(e, $"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: Web socket close exception (state: {webSocket.State}).");
                                    }
                                    break;

                                case WebSocketMessageType.Text:
                                    if (result.Count > 0)
                                    {
                                        stringBuilder.Append(Encoding.UTF8.GetString(bytes, 0, result.Count));
                                    }
                                    break;

                                case WebSocketMessageType.Binary:
                                    Logger?.LogWarning($"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: Received unsupported binary message type (state: {webSocket.State}).");

                                    try
                                    {
                                        await webSocket.CloseOutputAsync(WebSocketCloseStatus.InvalidMessageType, string.Empty, CancellationToken.None)
                                            .ConfigureAwait(false);
                                    }
                                    catch (Exception e)
                                    {
                                        Logger?.LogWarning(e, $"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: Web socket close exception (state: {webSocket.State}).");
                                    }
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException(nameof(result.MessageType), $"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: Unknown result message type ({result.MessageType}).");
                            }
                        }
                        while (!result.EndOfMessage);
                    }
                    catch (OperationCanceledException) { /* ignore */ }
                    catch (Exception e)
                    {
                        if (!token.IsCancellationRequested)
                        {
                            Logger?.LogWarning(e, $"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: Web socket receive exception (state: {webSocket.State}).");
                            throw;
                        }
                    }

                    if (token.IsCancellationRequested || webSocket.State != WebSocketState.Open)
                        break;

                    var json = stringBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        OnMessage(json, uri.AbsoluteUri);
                    }
                    else
                    {
                        Logger?.LogWarning($"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: Received empty JSON message (state: {webSocket.State}).");
                    }
                }
            }
            finally
            {
                webSocket?.Dispose();

                lock (_sync) { IsStreaming = false; }

                OnClose();

                Logger?.LogDebug($"{nameof(DefaultWebSocketClient)}.{nameof(StreamAsync)}: Task complete.  [thread: {Thread.CurrentThread.ManagedThreadId}]");
            }
        }

        #endregion Public Methods
    }
}
