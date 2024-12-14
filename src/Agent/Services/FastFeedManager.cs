using System.Collections.Concurrent;
using Agent;
using ProtoClient;
using com.chronoxor.simple;
using Disruptor;
using Disruptor.Dsl;
using Agent.Models;

namespace HFTbridge.Agent.Services
{
    public class FastFeedManager
    {
        private readonly SimpleProtoClient _feed;
        private readonly ConcurrentDictionary<string, SymbolInfo> _symbolInfoCache;
        private readonly Disruptor<FastMarketDataEvent> _fastFeedAnalysisPipe;
        private readonly Disruptor<LowLatencyTickDataEvent> _lowLatencyMDPath;
        private readonly string _tradingAccount;

        private readonly StoreMarketData _mdStore;

        public FastFeedManager(Disruptor<FastMarketDataEvent> fastFeedAnalysisPipe, StoreMarketData mdStore,
        Disruptor<LowLatencyTickDataEvent> lowLatencyMDPath
        )
        {
            _mdStore = mdStore;
            _fastFeedAnalysisPipe = fastFeedAnalysisPipe;
            _lowLatencyMDPath = lowLatencyMDPath;
            _symbolInfoCache = new ConcurrentDictionary<string, SymbolInfo>();
            AddSymbols();
            _feed = new SimpleProtoClient(AgentConfig.FastFeedHost, AgentConfig.FastFeedPort);
          // _feed = new SimpleProtoClient("207.167.96.187", 9011);
          // _feed = new SimpleProtoClient("207.167.96.187", 9010);
            _feed.ReceivedNotify_NewMdNotify += HandleNewTick;
            _tradingAccount = "INTERNAL.SPOT";
        }

        public void Start()
        {
            _feed.ConnectAndStart();
        }

        private void AddSymbols()
        {
            // AddSymbol("XAUUSD", 2);
            // AddSymbol("EURUSD", 5);
// --------
            AddSymbol("XBR/USD", 2);
            AddSymbol("XTI/USD", 2); 
            AddSymbol("XAG/USD", 2);
            AddSymbol("XAU/USD", 2);

            AddSymbol("J225", 2);
            AddSymbol("GDAXI", 2);
            AddSymbol("NDX", 2);
            AddSymbol("SPX", 2);
            AddSymbol("WS30", 1);

            AddSymbol("CAD/JPY", 3);
            AddSymbol("USD/JPY", 3);
            AddSymbol("GBP/JPY", 3);
            AddSymbol("AUD/JPY", 3);
            AddSymbol("CHF/JPY", 3);
            AddSymbol("EUR/JPY", 3);

            AddSymbol("AUD/NZD", 5);
            AddSymbol("NZD/USD", 5);
            AddSymbol("USD/CHF", 5);
            AddSymbol("USD/CAD", 5);
            AddSymbol("GBP/USD", 5);
            AddSymbol("GBP/CHF", 5);
            AddSymbol("GBP/CAD", 5);
            AddSymbol("EUR/USD", 5);
            AddSymbol("EUR/GBP", 5);
            AddSymbol("EUR/CHF", 5);
            AddSymbol("EUR/CAD", 5);
            AddSymbol("EUR/AUD", 5);
            AddSymbol("AUD/USD", 5);




            //-----
            //  AddSymbol("XBRUSD", 2);
            // AddSymbol("XTIUSD", 2);
            // AddSymbol("XAGUSD", 2);
            // AddSymbol("XAUUSD", 2);

            // AddSymbol("J225", 2);
            // AddSymbol("GDAXI", 2);
            // AddSymbol("NDX", 2);
            // AddSymbol("SPX", 2);
            // AddSymbol("WS30", 1);

            // AddSymbol("CADJPY", 3);
            // AddSymbol("USDJPY", 3);
            // AddSymbol("GBPJPY", 3);
            // AddSymbol("AUDJPY", 3);
            // AddSymbol("CHFJPY", 3);
            // AddSymbol("EURJPY", 3);

            // AddSymbol("AUDNZD", 5);
            // AddSymbol("NZDUSD", 5);
            // AddSymbol("USDCHF", 5);
            // AddSymbol("USDCAD", 5);
            // AddSymbol("GBPUSD", 5);
            // AddSymbol("GBPCHF", 5);
            // AddSymbol("GBPCAD", 5);
            // AddSymbol("EURUSD", 5);
            // AddSymbol("EURGBP", 5);
            // AddSymbol("EURCHF", 5);
            // AddSymbol("EURCAD", 5);
            // AddSymbol("EURAUD", 5);
            // AddSymbol("AUDUSD", 5);
        }

        private void AddSymbol(string symbolKey, int digits)
        {
            var symbolRouting = symbolKey.Replace("/", "");
            _symbolInfoCache[symbolKey] = new SymbolInfo(symbolRouting, digits);
            _mdStore.Add("PUBLIC", "INTERNAL.SPOT", symbolKey, symbolRouting, digits);
        }

        private void HandleNewTick(NewMdNotify notify)
        {
          

            if (_symbolInfoCache.TryGetValue(notify.symbolkey, out var symbolInfo))
            {

                using (var scope = _lowLatencyMDPath.PublishEvent())
                {
                    var data = scope.Event();
                    data.Publish("FAST.FEED",symbolInfo.SymbolRouting, notify.symbolkey,notify.ask, notify.bid);
                // data.Publish(notify.symbolkey, symbolInfo.SymbolRouting, notify.ask, notify.bid, symbolInfo.Digits, symbolInfo.TickCounter);
                }

                // Update symbol information
                symbolInfo.Update(notify.ask, notify.bid);

                // Publish the event for further analysis
                using (var scope = _fastFeedAnalysisPipe.PublishEvent())
                {
                    var data = scope.Event();
                    data.Publish(notify.symbolkey, symbolInfo.SymbolRouting, notify.ask, notify.bid, symbolInfo.Digits, symbolInfo.TickCounter);
                }

                // Log or trigger further actions based on updated symbol information
                // You can access symbolInfo.Spread, symbolInfo.Average, symbolInfo.LastUpdated here if needed
            }
        }


        private class SymbolInfo
        {
            public string SymbolRouting { get; }
            public int Digits { get; }
            public double Ask { get; private set; }
            public double Bid { get; private set; }
            public double Spread => Math.Round(Ask - Bid, Digits);
            public double Average => Math.Round((Ask + Bid) / 2, Digits);
            public DateTime LastUpdated { get; private set; }

            public long TickCounter {get;set;}

            public SymbolInfo(string symbolRouting, int digits)
            {
                SymbolRouting = symbolRouting;
                Digits = digits;
            }

            public void Update(double ask, double bid)
            {
                Ask = ask;
                Bid = bid;
                LastUpdated = DateTime.UtcNow;
                TickCounter ++;
            }
        }

    }
}
