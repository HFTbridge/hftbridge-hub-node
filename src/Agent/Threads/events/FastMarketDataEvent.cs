using HFTbridge.Msg;

namespace HFTbridge.Agent
{
    public class FastMarketDataEvent
    {
        public long IncomingTickTs { get; set; }
        public long ProcessedTickTs { get; set; }
        public double ProcessingMs { get; set; }

        public string SymbolKey { get; set; }
        public string SymbolRouting { get; set; }

        public int Digits { get; set; }
        public double Ask { get; set; }
        public double Bid { get; set; }

        public FastMarketDataEvent()
        {
            
        }

        public void CalculateProcessingTimeMs()
        {
            ProcessedTickTs = DateTime.UtcNow.Ticks;
            ProcessingMs = (ProcessedTickTs - IncomingTickTs) / 10000.0; // Convert ticks to milliseconds
           // ProcessingMs = Math.Round(ProcessingMs, 3); // Format up to 0.001 ms
        }

        public void Publish(string symbolKey, string symbolRouting, double ask, double bid, int digits)
        {
            IncomingTickTs = DateTime.UtcNow.Ticks;
            ProcessedTickTs = 0;
            SymbolKey = symbolKey;
            Ask = ask;
            Bid = bid;
            Digits = digits;
            SymbolRouting = symbolRouting;
        }

        // Create a dummy MsgMDRouting object
        public SubMsgMDRouting ToMsg()
        {

            var msg = new SubMsgMDRouting(
                IncomingTickTs: this.IncomingTickTs,
                ProcessedTickTs: this.ProcessedTickTs,
                ProcessingMs: this.ProcessingMs,
                SymbolKey: this.SymbolKey,
                SymbolRouting: this.SymbolRouting,
                Digits: this.Digits,
                Ask: this.Ask,
                Bid: this.Bid,
                AveragePrice: (this.Ask + this.Bid) / 2,
                Spread: this.Ask - this.Bid,
                TradingConnectionQuotes: new SubMsgMDRoutingItem[0]
            );

            return msg;
        }
    }
}
