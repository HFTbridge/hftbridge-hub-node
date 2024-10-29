using HFTbridge.Msg;

namespace Agent.Models
{
    public class StoreMarketDataItem
    {
        public string MarketDataKey { get; set; }

        public long Ts { get; set; }
        public long TickNumber { get; set; }
        public string OrganizationId { get; set; }
        public string TradingAccountId { get; set; }
        public string SymbolKey { get; set; }
        public string SymbolRouting { get; set; }
        public int Digits { get; set; }
        public double Ask { get; set; }
        public double Bid { get; set; }
        public double AveragePrice { get; set; }
        public double Spread { get; set; }
        public int Tps { get; set; }
        public int Tpm { get; set; }
        public double Delta1M { get; set; }
        public double Delta5M { get; set; }

        public StoreMarketDataItem(string organizationId, string tradingAccountId, string symbolKey, string symbolRouting, int digits)
        {
            MarketDataKey = tradingAccountId + "|" + symbolKey;
            OrganizationId = organizationId;
            TradingAccountId = tradingAccountId;
            SymbolKey = symbolKey;
            SymbolRouting = symbolRouting;
            Digits = digits;
        }

        public void Update(double ask, double bid)
        {
            Ask = ask;
            Bid = bid;
            AveragePrice = (ask + bid) / 2;
            Spread = ask - bid;
            TickNumber++;
            Ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    public class StoreMarketData
    {
        private long _ts;
        private int _totalTps;
        private int _symbolsCount;
        private string _nodeId;

        private Dictionary<string, StoreMarketDataItem> _store;

        public StoreMarketData()
        {
            _nodeId = "N/A";
            _store = new Dictionary<string, StoreMarketDataItem>();
        }

        public void Add(string organizationId, string tradingAccountId, string symbolKey, string symbolRouting, int digits)
        {
            string key = tradingAccountId + "|" + symbolKey;
            if (!_store.ContainsKey(key))
            {
                var marketDataItem = new StoreMarketDataItem(organizationId, tradingAccountId, symbolKey, symbolRouting, digits);
                _store.Add(key, marketDataItem);
                _symbolsCount++;
            }
        }

        public void Delete(string tradingAccountId, string symbolKey)
        {
            string key = tradingAccountId + "|" + symbolKey;
            if (_store.ContainsKey(key))
            {
                _store.Remove(key);
                _symbolsCount--;
            }
        }

        public void Update(string tradingAccountId, string symbolKey, double ask, double bid)
        {
            string key = tradingAccountId + "|" + symbolKey;
            if (_store.ContainsKey(key))
            {
                _store[key].Update(ask, bid);
                _totalTps++;
                _ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }
        

        public MsgSnapshotSingleAgentMarketData GetSnapshot()
        {
            // Collect the quotes from the store
            var quotes = _store.Values.Select(item => new SubMsgSnapshotSingleAgentMarketDataQuote(
                item.Ts,
                item.TickNumber,
                item.OrganizationId,
                item.TradingAccountId,
                item.SymbolKey,
                item.SymbolRouting,
                item.Digits,
                item.Ask,
                item.Bid,
                item.AveragePrice,
                item.Spread,
                item.Tps,
                item.Tpm,
                item.Delta1M,
                item.Delta5M
            )).ToList();

            // Create the snapshot model
            var snapshot = new MsgSnapshotSingleAgentMarketData(
                Ts: _ts,
                TotalTps: _totalTps,
                SymbolsCount: _symbolsCount,
                NodeId: _nodeId,
                Quotes: quotes
            );

            return snapshot;
        }
    }
}
