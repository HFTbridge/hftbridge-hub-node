using HFTbridge.Msg;
using HFTbridge.TC.Shared.Models;
using System.Collections.Generic;

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

        public List<TCSymbolQuote> TradinConnectionsQuotes {get;set;}

        public FastMarketDataEvent()
        {
            TradinConnectionsQuotes = new List<TCSymbolQuote>();
        }

        public void CalculateProcessingTimeMs()
        {
            ProcessedTickTs = DateTime.UtcNow.Ticks;
            ProcessingMs = (ProcessedTickTs - IncomingTickTs) / 10000.0;

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
                TradingConnectionQuotes: ConvertToMDRoutingItems(TradinConnectionsQuotes)
            );
            // foreach (var item in msg.TradingConnectionQuotes)
            // {
            //     Console.WriteLine(item);
            // }
            return msg;
        }

        public static SubMsgMDRoutingItem[] ConvertToMDRoutingItems(List<TCSymbolQuote> symbolQuotes)
        {
            return symbolQuotes.Select(sq => new SubMsgMDRoutingItem(
                Ts: sq.Ts,
                OrganizationId: sq.OrganizationId,
                TradingAccountId: sq.TradingAccountId,
                SymbolKey: sq.Symbolkey,
                SymbolRouting: sq.SymbolRouting,
                Digits: (int)sq.Digits,
                Ask: sq.Ask,
                Bid: sq.Bid,
                AveragePrice: sq.AvgPrice,
                Spread: sq.Spread,
                AskOffset: 0, // Placeholder, replace with real value if available
                BidOffset: 0, // Placeholder, replace with real value if available
                AskAfterOffset: sq.Ask, // Placeholder, logic might be needed here
                BidAfterOffset: sq.Bid, // Placeholder, logic might be needed here
                BuyGap: 13, // Placeholder, replace with real value if available
                SellGap: 0, // Placeholder, replace with real value if available
                IsBuyGap: true, // Placeholder, replace with real value if available
                IsSellGap: false  // Placeholder, replace with real value if available
            )).ToArray();
        }
    }
}
