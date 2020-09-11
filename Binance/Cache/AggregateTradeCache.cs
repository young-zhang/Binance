﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Binance.Client;
using Microsoft.Extensions.Logging;

namespace Binance.Cache
{
    /// <summary>
    /// The default <see cref="IAggregateTradeCache"/> implemenation.
    /// </summary>
    public class AggregateTradeCache : AggregateTradeCache<IAggregateTradeClient>, IAggregateTradeCache
    {
        /// <summary>
        /// Default constructor provides default <see cref="IBinanceApi"/>
        /// and default <see cref="IAggregateTradeClient"/>, but no logger.
        /// </summary>
        public AggregateTradeCache()
            : this(new BinanceApi(), new AggregateTradeClient())
        { }

        /// <summary>
        /// The DI constructor.
        /// </summary>
        /// <param name="api">The Binance api (required).</param>
        /// <param name="client">The JSON client (required).</param>
        /// <param name="logger">The logger (optional).</param>
        public AggregateTradeCache(IBinanceApi api, IAggregateTradeClient client, ILogger<AggregateTradeCache> logger = null)
            : base(api, client, logger)
        { }
    }

    /// <summary>
    /// The default <see cref="IAggregateTradeCache{TClient}"/> implemenation.
    /// </summary>
    public abstract class AggregateTradeCache<TClient> : JsonClientCache<TClient, AggregateTradeEventArgs, AggregateTradeCacheEventArgs>, IAggregateTradeCache<TClient>
        where TClient : class, IAggregateTradeClient
    {
        #region Public Events

        public event EventHandler<EventArgs> OutOfSync;

        #endregion Public Events

        #region Public Properties

        public IEnumerable<AggregateTrade> Trades
        {
            get { lock (_sync) { return _trades?.ToArray() ?? new AggregateTrade[] { }; } }
        }

        public override IEnumerable<string> SubscribedStreams
        {
            get
            {
                return _symbol == null
                    ? new string[] { }
                    : new string[] { AggregateTradeClient.GetStreamName(_symbol) };
            }
        }

        #endregion Public Properties

        #region Private Fields

        private readonly Queue<AggregateTrade> _trades;

        private readonly object _sync = new object();

        private string _symbol;
        private int _limit;

        #endregion Private Fields

        #region Constructors

        protected AggregateTradeCache(IBinanceApi api, TClient client, ILogger<AggregateTradeCache<TClient>> logger = null)
            : base(api, client, logger)
        {
            _trades = new Queue<AggregateTrade>();
        }

        #endregion Constructors

        #region Public Methods

        public void Subscribe(string symbol, int limit, Action<AggregateTradeCacheEventArgs> callback)
        {
            Throw.IfNullOrWhiteSpace(symbol, nameof(symbol));

            if (limit < 0)
                throw new ArgumentException($"{GetType().Name}: {nameof(limit)} must be greater than or equal to 0.", nameof(limit));

            if (_symbol != null)
                throw new InvalidOperationException($"{GetType().Name}.{nameof(Subscribe)}: Already subscribed to a symbol: \"{_symbol}\"");

            _symbol = symbol.FormatSymbol();
            _limit = limit;

            OnSubscribe(callback);
            SubscribeToClient();
        }

        public override IJsonSubscriber Unsubscribe()
        {
            if (_symbol == null)
                return this;

            UnsubscribeFromClient();
            OnUnsubscribe();

            lock (_sync)
            {
                _trades.Clear();
            }

            _symbol = default;
            _limit = default;

            return this;
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void SubscribeToClient()
        {
            if (_symbol == null)
                return;

            Client.Subscribe(_symbol, ClientCallback);
        }

        protected override void UnsubscribeFromClient()
        {
            if (_symbol == null)
                return;

            Client.Unsubscribe(_symbol, ClientCallback);
        }

        protected override async ValueTask<AggregateTradeCacheEventArgs> OnActionAsync(AggregateTradeEventArgs @event, CancellationToken token = default)
        {
            var synchronize = false;

            // If trades have not been initialized or are out-of-sync (gap in data).
            lock (_sync)
            {
                if (_trades.Count == 0 || @event.Trade.Id > _trades.Last().Id + 1)
                {
                    if (_trades.Count > 0)
                    {
                        OutOfSync?.Invoke(this, EventArgs.Empty);
                    }

                    synchronize = true;
                }
            }

            if (synchronize)
            {
                await SynchronizeTradesAsync(_symbol, _limit, token)
                    .ConfigureAwait(false);
            }

            lock (_sync)
            {
                if (_trades.Count == 0 || @event.Trade.Id > _trades.Last().Id + 1)
                {
                    Logger?.LogError($"{GetType().Name} ({_symbol}): Failed to synchronize trades.  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                    return null;
                }

                // Ignore trades older than the latest trade in queue.
                if (@event.Trade.Id <= _trades.Last().Id)
                {
                    Logger?.LogDebug($"{GetType().Name} ({_symbol}): Ignoring event (trade ID: {@event.Trade.Id}).  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                    return null;
                }

                var removed = _trades.Dequeue();
                Logger?.LogTrace($"{GetType().Name} ({_symbol}): REMOVE aggregate trade (ID: {removed.Id}).  [thread: {Thread.CurrentThread.ManagedThreadId}]");

                _trades.Enqueue(@event.Trade);
                Logger?.LogTrace($"{GetType().Name} ({_symbol}): ADD aggregate trade (ID: {@event.Trade.Id}).  [thread: {Thread.CurrentThread.ManagedThreadId}]");

                return new AggregateTradeCacheEventArgs(_trades.ToArray());
            }
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Get latest trades.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="limit"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task SynchronizeTradesAsync(string symbol, int limit, CancellationToken token)
        {
            Logger?.LogInformation($"{GetType().Name} ({_symbol}): Synchronizing aggregate trades...  [thread: {Thread.CurrentThread.ManagedThreadId}]");

            var trades = await Api.GetAggregateTradesAsync(symbol, limit, token)
                .ConfigureAwait(false);

            lock (_sync)
            {
                _trades.Clear();
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var trade in trades)
                {
                    _trades.Enqueue(trade);
                }
            }

            // ReSharper disable once PossibleMultipleEnumeration
            Logger?.LogInformation($"{GetType().Name} ({_symbol}): Synchronization complete (latest trade ID: {trades.Last().Id}).  [thread: {Thread.CurrentThread.ManagedThreadId}]");
        }

        #endregion Private Methods
    }
}
