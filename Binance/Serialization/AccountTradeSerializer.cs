﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Binance.Serialization
{
    public class AccountTradeSerializer : IAccountTradeSerializer
    {
        private const string KeySymbol = "symbol";
        private const string KeyId = "id";
        private const string KeyOrderId = "orderId";
        private const string KeyPrice = "price";
        private const string KeyQuantity = "qty";
        private const string KeyQuoteQuantity = "quoteQty";
        private const string KeyCommission = "commission";
        private const string KeyCommissionAsset = "commissionAsset";
        private const string KeyTime = "time";
        private const string KeyIsBuyer = "isBuyer";
        private const string KeyIsMaker = "isMaker";
        private const string KeyIsBestPriceMatch = "isBestMatch";

        public virtual AccountTrade Deserialize(string json)
        {
            Throw.IfNullOrWhiteSpace(json, nameof(json));

            return DeserializeTrade(JObject.Parse(json));
        }

        public virtual IEnumerable<AccountTrade> DeserializeMany(string json, string symbol)
        {
            Throw.IfNullOrWhiteSpace(json, nameof(json));
            Throw.IfNullOrWhiteSpace(symbol, nameof(symbol));

            symbol = symbol.FormatSymbol();

            return JArray.Parse(json)
                .Select(jToken => DeserializeTrade(jToken, symbol))
                .ToArray();
        }

        public virtual string Serialize(AccountTrade trade)
        {
            Throw.IfNull(trade, nameof(trade));

            var jObject = new JObject
            {
                new JProperty(KeySymbol, trade.Symbol),
                new JProperty(KeyId, trade.Id),
                new JProperty(KeyOrderId, trade.OrderId),
                new JProperty(KeyPrice, trade.Price.ToString(CultureInfo.InvariantCulture)),
                new JProperty(KeyQuantity, trade.Quantity.ToString(CultureInfo.InvariantCulture)),
                new JProperty(KeyQuoteQuantity, trade.QuoteQuantity.ToString(CultureInfo.InvariantCulture)),
                new JProperty(KeyCommission, trade.Commission.ToString(CultureInfo.InvariantCulture)),
                new JProperty(KeyCommissionAsset, trade.CommissionAsset),
                new JProperty(KeyTime, trade.Time.ToTimestamp()),
                new JProperty(KeyIsBuyer, trade.IsBuyer),
                new JProperty(KeyIsMaker, trade.IsMaker),
                new JProperty(KeyIsBestPriceMatch, trade.IsBestPriceMatch)
            };

            return jObject.ToString(Formatting.None);
        }

        private static AccountTrade DeserializeTrade(JToken jToken, string symbol = null)
        {
            if (symbol == null)
            {
                return new AccountTrade(
                    jToken[KeySymbol].Value<string>(),
                    jToken[KeyId].Value<long>(),
                    jToken[KeyOrderId].Value<long>(),
                    jToken[KeyPrice].Value<decimal>(),
                    jToken[KeyQuantity].Value<decimal>(),
                    jToken[KeyQuoteQuantity].Value<decimal>(),
                    jToken[KeyCommission].Value<decimal>(),
                    jToken[KeyCommissionAsset].Value<string>(),
                    jToken[KeyTime].Value<long>().ToDateTime(),
                    jToken[KeyIsBuyer].Value<bool>(),
                    jToken[KeyIsMaker].Value<bool>(),
                    jToken[KeyIsBestPriceMatch].Value<bool>());
            }

            return new AccountTrade(
                symbol,
                jToken[KeyId].Value<long>(),
                jToken[KeyOrderId].Value<long>(),
                jToken[KeyPrice].Value<decimal>(),
                jToken[KeyQuantity].Value<decimal>(),
                jToken[KeyQuoteQuantity].Value<decimal>(),
                jToken[KeyCommission].Value<decimal>(),
                jToken[KeyCommissionAsset].Value<string>(),
                jToken[KeyTime].Value<long>().ToDateTime(),
                jToken[KeyIsBuyer].Value<bool>(),
                jToken[KeyIsMaker].Value<bool>(),
                jToken[KeyIsBestPriceMatch].Value<bool>());
        }
    }
}
