﻿// ReSharper disable once CheckNamespace
namespace Binance
{
    public sealed class Fill
    {
        #region Public Properties

        /// <summary>
        /// Get the price.
        /// </summary>
        public decimal Price { get; }

        /// <summary>
        /// Get the quantity.
        /// </summary>
        public decimal Quantity { get; }

        /// <summary>
        /// Get the commission (commission asset quantity).
        /// </summary>
        public decimal Commission { get; }

        /// <summary>
        /// Get the commission asset.
        /// </summary>
        public string CommissionAsset { get; }

        /// <summary>
        /// Get the <see cref="AccountTrade"/> ID.
        /// </summary>
        public long TradeId { get; }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="price">The price.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="commission">The commission.</param>
        /// <param name="commissionAsset">The commission asset.</param>
        /// <param name="tradeId">The account trade ID.</param>
        public Fill(decimal price, decimal quantity, decimal commission, string commissionAsset, long tradeId)
        {
            Price = price;
            Quantity = quantity;
            Commission = commission;
            CommissionAsset = commissionAsset;
            TradeId = tradeId;
        }

        #endregion Constructors
    }
}
