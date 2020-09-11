﻿using System;
using Binance.Client;

namespace Binance.Cache
{
    public interface IOrderBookCache : IOrderBookCache<IDepthClient>
    { }

    public interface IOrderBookCache<TClient> : IJsonClientCache<TClient, OrderBookCacheEventArgs>
        where TClient : IDepthClient
    {
        /// <summary>
        /// Order book out-of-sync event.
        /// </summary>
        event EventHandler<EventArgs> OutOfSync;

        /// <summary>
        /// The order book. Can be null if not yet synchronized or out-of-sync.
        /// </summary>
        OrderBook OrderBook { get; }

        /// <summary>
        /// Subscribe the web socket client to the symbol and link this cache to client.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="limit">The limit (optional, uses partial depth stream). Valid values are: 5, 10, or 20.</param>
        /// <param name="callback">An event callback (optional).</param>
        void Subscribe(string symbol, int limit, Action<OrderBookCacheEventArgs> callback);
    }
}
