﻿using System;
using System.Globalization;

// ReSharper disable once CheckNamespace
namespace Binance
{
    /// <summary>
    /// A symbol/price value object.
    /// </summary>
    public sealed class SymbolPrice : IEquatable<SymbolPrice>
    {
        #region Public Properties

        /// <summary>
        /// Get the symbol.
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Get the price value.
        /// </summary>
        public decimal Value { get; }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="value">The price value.</param>
        public SymbolPrice(string symbol, decimal value)
        {
            Throw.IfNullOrWhiteSpace(symbol, nameof(symbol));

            if (value < 0)
                throw new ArgumentException($"{nameof(SymbolPrice)} value must not be less than 0.", nameof(value));

            Symbol = symbol.FormatSymbol();
            Value = value;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Display price value as string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value.ToString("0.00000000", CultureInfo.InvariantCulture);
        }

        #endregion Public Methods

        #region IEquatable

        public bool Equals(SymbolPrice other)
        {
            if (other == null)
                return false;

            return other.Symbol == Symbol
                && other.Value == Value;
        }

        #endregion IEquatable
    }
}
