﻿using System;

// ReSharper disable once CheckNamespace
namespace Binance
{
    /// <summary>
    /// Order book (depth of market) price and quantity.
    /// </summary>
    public sealed class OrderBookPriceLevel : IEquatable<OrderBookPriceLevel>
    {
        #region Public Properties

        /// <summary>
        /// Get the price.
        /// </summary>
        public decimal Price { get; }

        /// <summary>
        /// Get the aggregate quantity.
        /// </summary>
        public decimal Quantity { get; internal set; }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Construct order book level.
        /// </summary>
        /// <param name="price">The price.</param>
        /// <param name="quantity">The aggregate quantity.</param>
        public OrderBookPriceLevel(decimal price, decimal quantity)
        {
            if (price < 0)
                throw new ArgumentException($"{nameof(OrderBookPriceLevel)} price must greater than or equal to 0.", nameof(price));
            if (quantity < 0)
                throw new ArgumentException($"{nameof(OrderBookPriceLevel)} quantity must be greater than or equal to 0.", nameof(quantity));

            Price = price;
            Quantity = quantity;
        }

        #endregion Constructors

        #region IEquatable

        public bool Equals(OrderBookPriceLevel other)
        {
            if (other == null)
                return false;

            return other.Price == Price
                && other.Quantity == Quantity;
        }

        #endregion IEquatable
    }
}
