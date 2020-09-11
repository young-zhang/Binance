﻿using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Binance.Client
{
    /// <summary>
    /// The default <see cref="ICandlestickClient"/> implementation.
    /// </summary>
    public class CandlestickClient : JsonClient<CandlestickEventArgs>, ICandlestickClient
    {
        #region Public Events

        public event EventHandler<CandlestickEventArgs> Candlestick;

        #endregion Public Events

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger"></param>
        public CandlestickClient(ILogger<CandlestickClient> logger = null)
            : base(logger)
        { }

        #endregion Construtors

        #region Public Methods

        /// <summary>
        /// Convert symbol and <see cref="CandlestickInterval"/> to stream name.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static string GetStreamName(string symbol, CandlestickInterval interval)
        {
            Throw.IfNullOrEmpty(symbol, nameof(symbol));

            return $"{symbol.ToLowerInvariant()}@kline_{interval.AsString()}";
        }

        public virtual ICandlestickClient Subscribe(string symbol, CandlestickInterval interval, Action<CandlestickEventArgs> callback)
        {
            Throw.IfNullOrWhiteSpace(symbol, nameof(symbol));

            symbol = symbol.FormatSymbol();

            Logger?.LogDebug($"{nameof(CandlestickClient)}.{nameof(Subscribe)}: \"{symbol}\" \"{interval.AsString()}\" (callback: {(callback == null ? "no" : "yes")}).  [thread: {Thread.CurrentThread.ManagedThreadId}]");

            SubscribeStream(GetStreamName(symbol, interval), callback);

            return this;
        }

        public virtual ICandlestickClient Unsubscribe(string symbol, CandlestickInterval interval, Action<CandlestickEventArgs> callback)
        {
            Throw.IfNullOrWhiteSpace(symbol, nameof(symbol));

            symbol = symbol.FormatSymbol();

            Logger?.LogDebug($"{nameof(CandlestickClient)}.{nameof(Unsubscribe)}: \"{symbol}\" \"{interval.AsString()}\" (callback: {(callback == null ? "no" : "yes")}).  [thread: {Thread.CurrentThread.ManagedThreadId}]");

            UnsubscribeStream(GetStreamName(symbol, interval), callback);

            return this;
        }

        public new virtual ICandlestickClient Unsubscribe() => (ICandlestickClient)base.Unsubscribe();

        #endregion Public Methods

        #region Protected Methods

        protected override void HandleMessage(IEnumerable<Action<CandlestickEventArgs>> callbacks, string stream, string json)
        {
            try
            {
                var jObject = JObject.Parse(json);

                var eventType = jObject["e"].Value<string>();

                if (eventType == "kline")
                {
                    //var symbol = jObject["s"].Value<string>();
                    var eventTime = jObject["E"].Value<long>().ToDateTime();

                    var kLine = jObject["k"];

                    var firstTradeId = kLine["f"].Value<long>();
                    var lastTradeId = kLine["L"].Value<long>();

                    var isFinal = kLine["x"].Value<bool>();

                    var candlestick = new Candlestick(
                        kLine["s"].Value<string>(),  // symbol
                        kLine["i"].Value<string>()   // interval
                            .ToCandlestickInterval(),
                        kLine["t"].Value<long>()     // open time
                            .ToDateTime(),
                        kLine["o"].Value<decimal>(), // open
                        kLine["h"].Value<decimal>(), // high
                        kLine["l"].Value<decimal>(), // low
                        kLine["c"].Value<decimal>(), // close
                        kLine["v"].Value<decimal>(), // volume
                        kLine["T"].Value<long>()     // close time
                            .ToDateTime(),
                        kLine["q"].Value<decimal>(), // quote asset volume
                        kLine["n"].Value<long>(),    // number of trades
                        kLine["V"].Value<decimal>(), // taker buy base asset volume (volume of active buy)
                        kLine["Q"].Value<decimal>()  // taker buy quote asset volume (quote volume of active buy)
                    );

                    var eventArgs = new CandlestickEventArgs(eventTime, candlestick, firstTradeId, lastTradeId, isFinal);

                    try
                    {
                        if (callbacks != null)
                        {
                            foreach (var callback in callbacks)
                                callback(eventArgs);
                        }
                        Candlestick?.Invoke(this, eventArgs);
                    }
                    catch (OperationCanceledException) { /* ignore */ }
                    catch (Exception e)
                    {
                        Logger?.LogWarning(e, $"{nameof(CandlestickClient)}.{nameof(HandleMessage)}: Unhandled candlestick event handler exception.");
                    }
                }
                else
                {
                    Logger?.LogWarning($"{nameof(CandlestickClient)}.{nameof(HandleMessage)}: Unexpected event type ({eventType}).");
                }
            }
            catch (OperationCanceledException) { /* ignore */ }
            catch (Exception e)
            {
                Logger?.LogError(e, $"{nameof(CandlestickClient)}.{nameof(HandleMessage)}");
            }
        }

        #endregion Protected Methods
    }
}
