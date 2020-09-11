﻿using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Binance.Client
{
    /// <summary>
    /// The default <see cref="IAggregateTradeClient"/> implementation.
    /// </summary>
    public class AggregateTradeClient : JsonClient<AggregateTradeEventArgs>, IAggregateTradeClient
    {
        #region Public Events

        public event EventHandler<AggregateTradeEventArgs> AggregateTrade;

        #endregion Public Events

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger"></param>
        public AggregateTradeClient(ILogger<AggregateTradeClient> logger = null)
            : base(logger)
        { }

        #endregion Construtors

        #region Public Methods

        /// <summary>
        /// Convert symbol to stream name.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static string GetStreamName(string symbol)
        {
            Throw.IfNull(symbol, nameof(symbol));

            return $"{symbol.ToLowerInvariant()}@aggTrade";
        }

        public virtual IAggregateTradeClient Subscribe(string symbol, Action<AggregateTradeEventArgs> callback)
        {
            Throw.IfNullOrWhiteSpace(symbol, nameof(symbol));

            symbol = symbol.FormatSymbol();

            Logger?.LogDebug($"{nameof(AggregateTradeClient)}.{nameof(Subscribe)}: \"{symbol}\" (callback: {(callback == null ? "no" : "yes")}).  [thread: {Thread.CurrentThread.ManagedThreadId}]");

            SubscribeStream(GetStreamName(symbol), callback);

            return this;
        }

        public virtual IAggregateTradeClient Unsubscribe(string symbol, Action<AggregateTradeEventArgs> callback)
        {
            Throw.IfNullOrWhiteSpace(symbol, nameof(symbol));

            symbol = symbol.FormatSymbol();

            Logger?.LogDebug($"{nameof(AggregateTradeClient)}.{nameof(Unsubscribe)}: \"{symbol}\" (callback: {(callback == null ? "no" : "yes")}).  [thread: {Thread.CurrentThread.ManagedThreadId}]");

            UnsubscribeStream(GetStreamName(symbol), callback);

            return this;
        }

        public new virtual IAggregateTradeClient Unsubscribe() => (IAggregateTradeClient)base.Unsubscribe();

        #endregion Public Methods

        #region Protected Methods

        protected override void HandleMessage(IEnumerable<Action<AggregateTradeEventArgs>> callbacks, string stream, string json)
        {
            try
            {
                var jObject = JObject.Parse(json);

                var eventType = jObject["e"].Value<string>();

                if (eventType == "aggTrade")
                {
                    var eventTime = jObject["E"].Value<long>().ToDateTime();

                    var trade = new AggregateTrade(
                        jObject["s"].Value<string>(),  // symbol
                        jObject["a"].Value<long>(),    // aggregate trade ID
                        jObject["p"].Value<decimal>(), // price
                        jObject["q"].Value<decimal>(), // quantity
                        jObject["f"].Value<long>(),    // first trade ID
                        jObject["l"].Value<long>(),    // last trade ID
                        jObject["T"].Value<long>()     // trade time
                            .ToDateTime(),
                        jObject["m"].Value<bool>(),    // is buyer the market maker?
                        jObject["M"].Value<bool>());   // is best price match?

                    var eventArgs = new AggregateTradeEventArgs(eventTime, trade);

                    try
                    {
                        if (callbacks != null)
                        {
                            foreach (var callback in callbacks)
                                callback(eventArgs);
                        }

                        AggregateTrade?.Invoke(this, eventArgs);
                    }
                    catch (OperationCanceledException) { /* ignore */ }
                    catch (Exception e)
                    {
                        Logger?.LogWarning(e, $"{nameof(AggregateTradeClient)}.{nameof(HandleMessage)}: Unhandled aggregate trade event handler exception.  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                    }
                }
                else
                {
                    Logger?.LogWarning($"{nameof(AggregateTradeClient)}.{nameof(HandleMessage)}: Unexpected event type ({eventType}).  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                }
            }
            catch (OperationCanceledException) { /* ignore */ }
            catch (Exception e)
            {
                Logger?.LogError(e, $"{nameof(AggregateTradeClient)}.{nameof(HandleMessage)}: Failed.  [thread: {Thread.CurrentThread.ManagedThreadId}]");
            }
        }

        #endregion Protected Methods
    }
}
