﻿using System;

// ReSharper disable once CheckNamespace
namespace Binance.Client
{
    /// <summary>
    /// Order execution event arguments.
    /// </summary>
    public abstract class OrderExecutionEventArgs : UserDataEventArgs
    {
        #region Public Properties

        /// <summary>
        /// Get the order.
        /// </summary>
        public Order Order { get; }

        /// <summary>
        /// Get the order execution type.
        /// </summary>
        public OrderExecutionType OrderExecutionType { get; }

        /// <summary>
        /// Get the order rejected reason.
        /// </summary>
        public string OrderRejectedReason { get; }

        /// <summary>
        /// Get the new client order ID.
        /// </summary>
        public string NewClientOrderId { get; }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="time">The event time.</param>
        /// <param name="order">The order.</param>
        /// <param name="orderExecutionType">The order execution type.</param>
        /// <param name="orderRejectedReason">The order rejected reason.</param>
        /// <param name="newClientOrderId">The new client order ID.</param>
        protected OrderExecutionEventArgs(DateTime time, Order order, OrderExecutionType orderExecutionType, string orderRejectedReason, string newClientOrderId)
            : base(time)
        {
            Throw.IfNull(order, nameof(order));
            Throw.IfNullOrWhiteSpace(orderRejectedReason, nameof(orderRejectedReason));

            Order = order;
            OrderExecutionType = orderExecutionType;
            OrderRejectedReason = orderRejectedReason;
            NewClientOrderId = newClientOrderId;
        }

        #endregion Constructors
    }
}
