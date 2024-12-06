namespace HFTbridge.Agent
{
    public class LowLatencyTickDataEvent
    {
        // Feed Mapping
        public string FeedNameMapping {get;set;}
        public string FeedSymbolKeyMapping {get;set;}
        public string FeedSymbolRoutingMapping {get;set;}

        // Time Management
        public DateTime IncomingTickTs { get; set; }
        public DateTime ProcessedTickTs { get; set; }
        public double ProcessingMs { get; set; }

        // Market Data
        public double Ask { get; set; }
        public double Bid { get; set; }

        public SubMsgMDRouting Msg {get;set;}

        public long GlobalProcessedTickCounter {get;set;}

        public bool ToBeProcessed  {get;set;}


        public LowLatencyTickDataEvent()
        {
            ToBeProcessed = false;
        }

        public void Publish(SubMsgMDRouting msg)
        {
            Msg = msg;
            //Console.WriteLine(msg.ToString());
        }


        public void Publish(string feed, string symbolRouting, string symbolKey,  double ask, double bid)
        {
            IncomingTickTs = DateTime.UtcNow;
            FeedNameMapping = feed;
            FeedSymbolKeyMapping = symbolKey;
            FeedSymbolRoutingMapping = symbolRouting;
            Ask = ask;
            Bid = bid;
            ToBeProcessed = true;
            Msg = default;
        }

        public string GetMatchingSymbolID()
        {
            return $"{FeedNameMapping}|{FeedSymbolKeyMapping}";
        }

        public override string ToString()
        {
            return $"UTC {IncomingTickTs:HH:mm:ss.ffffff}|{FeedNameMapping}|{FeedSymbolRoutingMapping}|{FeedSymbolKeyMapping}|Ask={Ask}|Bid={Bid}";
        }

    }
}
