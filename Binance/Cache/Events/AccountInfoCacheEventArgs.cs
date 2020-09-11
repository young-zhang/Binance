﻿// ReSharper disable once CheckNamespace
namespace Binance.Cache
{
    public sealed class AccountInfoCacheEventArgs : CacheEventArgs
    {
        #region Public Properties

        /// <summary>
        /// Get the account information.
        /// </summary>
        public AccountInfo AccountInfo { get; }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="accountInfo">The account information.</param>
        public AccountInfoCacheEventArgs(AccountInfo accountInfo)
        {
            Throw.IfNull(accountInfo, nameof(accountInfo));

            AccountInfo = accountInfo;
        }

        #endregion Constructors
    }
}
