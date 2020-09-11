﻿using System;

// ReSharper disable once CheckNamespace
namespace Binance
{
    /// <summary>
    /// An account asset balance.
    /// </summary>
    public sealed class AccountBalance
    {
        #region Public Properties

        /// <summary>
        /// Get the asset.
        /// </summary>
        public string Asset { get; }

        /// <summary>
        /// Get the free (available) amount.
        /// </summary>
        public decimal Free { get; }

        /// <summary>
        /// Get the locked (on hold) amount.
        /// </summary>
        public decimal Locked { get; }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="free">The free amount.</param>
        /// <param name="locked">The locked amount.</param>
        public AccountBalance(string asset, decimal free, decimal locked)
        {
            Throw.IfNullOrWhiteSpace(asset, nameof(asset));

            if (free < 0)
                throw new ArgumentException($"{nameof(AccountBalance)}: amount must not be less than 0.", nameof(free));
            if (locked < 0)
                throw new ArgumentException($"{nameof(AccountBalance)}: amount must not be less than 0.", nameof(locked));

            Asset = asset.FormatSymbol();
            Free = free;
            Locked = locked;
        }

        #endregion Constructors
    }
}
