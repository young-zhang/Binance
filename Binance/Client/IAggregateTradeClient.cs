﻿using System;

namespace Binance.Client
{
    public interface IAggregateTradeClient : IJsonClient
    {
        /// <summary>
        /// The aggregate trade event. Receive aggregate trade events for all
        /// subscribed symbols.
        /// </summary>
        event EventHandler<AggregateTradeEventArgs> AggregateTrade;

        /// <summary>
        /// Subscribe to the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to subscribe.</param>
        /// <param name="callback">An event callback (optional).</param>
        /// <returns></returns>
        IAggregateTradeClient Subscribe(string symbol, Action<AggregateTradeEventArgs> callback);

        /// <summary>
        /// Unsubscribe a callback from a symbol. If no callback is specified,
        /// unsubscribe from symbol (all callbacks).
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        IAggregateTradeClient Unsubscribe(string symbol, Action<AggregateTradeEventArgs> callback);

        /// <summary>
        /// Unsubscribe from all symbols (and callbacks).
        /// </summary>
        /// <returns></returns>
        new IAggregateTradeClient Unsubscribe();
    }
}
