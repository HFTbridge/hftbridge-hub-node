namespace Agent.Models
{
    public class LowLatencyMarketDataStore
    {
        public Dictionary<string,LowLatencyMarketDataRouting> RoutingStore {get;set;}
        public LowLatencyMarketDataStore()
        {
            RoutingStore = new Dictionary<string, LowLatencyMarketDataRouting>();
            LoadRoutings();
        }

        public void Update(string FeedID, string SymbolRouting, string SymbolKey, double Ask, double Bid)
        {
            
        }

        public void Update(LowLatencyTickDataEvent data)
        {
            //check if Symbol routing exists in the dict
            if (RoutingStore.TryGetValue(data.FeedSymbolRoutingMapping, out var routingItem))
            {
                routingItem.Update(data);
                //Console.WriteLine(data.ToString());
            }

            
        }

        public void LoadRoutings()
        {
            var FeedID = "FAST.FEED";
            RoutingStore["XBRUSD"] = new LowLatencyMarketDataRouting(FeedID,"XBRUSD",2);
            RoutingStore["XTIUSD"] = new LowLatencyMarketDataRouting(FeedID,"XTIUSD",2);
            RoutingStore["XAGUSD"] = new LowLatencyMarketDataRouting(FeedID,"XAGUSD",2);
            RoutingStore["XAUUSD"] = new LowLatencyMarketDataRouting(FeedID,"XAUUSD",2);
            RoutingStore["J225"] = new LowLatencyMarketDataRouting(FeedID,"J225",2);
            RoutingStore["GDAXI"] = new LowLatencyMarketDataRouting(FeedID,"GDAXI",2);
            RoutingStore["NDX"] = new LowLatencyMarketDataRouting(FeedID,"NDX",2);
            RoutingStore["SPX"] = new LowLatencyMarketDataRouting(FeedID,"SPX",2);
            RoutingStore["WS30"] = new LowLatencyMarketDataRouting(FeedID,"WS30",1);
            RoutingStore["CADJPY"] = new LowLatencyMarketDataRouting(FeedID,"CADJPY",3);
            RoutingStore["USDJPY"] = new LowLatencyMarketDataRouting(FeedID,"USDJPY",3);
            RoutingStore["GBPJPY"] = new LowLatencyMarketDataRouting(FeedID,"GBPJPY",3);
            RoutingStore["AUDJPY"] = new LowLatencyMarketDataRouting(FeedID,"AUDJPY",3);
            RoutingStore["CHFJPY"] = new LowLatencyMarketDataRouting(FeedID,"CHFJPY",3);
            RoutingStore["EURJPY"] = new LowLatencyMarketDataRouting(FeedID,"EURJPY",3);
            RoutingStore["AUDNZD"] = new LowLatencyMarketDataRouting(FeedID,"AUDNZD",5);
            RoutingStore["NZDUSD"] = new LowLatencyMarketDataRouting(FeedID,"NZDUSD",5);
            RoutingStore["USDCHF"] = new LowLatencyMarketDataRouting(FeedID,"USDCHF",5);
            RoutingStore["USDCAD"] = new LowLatencyMarketDataRouting(FeedID,"USDCAD",5);
            RoutingStore["GBPUSD"] = new LowLatencyMarketDataRouting(FeedID,"GBPUSD",5);
            RoutingStore["GBPCHF"] = new LowLatencyMarketDataRouting(FeedID,"GBPCHF",5);
            RoutingStore["GBPCAD"] = new LowLatencyMarketDataRouting(FeedID,"GBPCAD",5);
            RoutingStore["EURUSD"] = new LowLatencyMarketDataRouting(FeedID,"EURUSD",5);
            RoutingStore["EURGBP"] = new LowLatencyMarketDataRouting(FeedID,"EURGBP",5);
            RoutingStore["EURCHF"] = new LowLatencyMarketDataRouting(FeedID,"EURCHF",5);
            RoutingStore["EURCAD"] = new LowLatencyMarketDataRouting(FeedID,"EURCAD",5);
            RoutingStore["XBRUSD"] = new LowLatencyMarketDataRouting(FeedID,"XBRUSD",5);
            RoutingStore["EURAUD"] = new LowLatencyMarketDataRouting(FeedID,"EURAUD",5);
            RoutingStore["AUDUSD"] = new LowLatencyMarketDataRouting(FeedID,"AUDUSD",5);

        }
    }
}