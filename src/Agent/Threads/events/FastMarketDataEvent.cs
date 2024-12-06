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

        public long TickCounter {get;set;}


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

        public void Publish(string symbolKey, string symbolRouting, double ask, double bid, int digits, long tickCounter)
        {
            IncomingTickTs = DateTime.UtcNow.Ticks;
            ProcessedTickTs = 0;
            SymbolKey = symbolKey;
            Ask = ask;
            Bid = bid;
            Digits = digits;
            SymbolRouting = symbolRouting;
            TickCounter = tickCounter;
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
                AveragePrice: Math.Round((this.Ask + this.Bid) / 2,this.Digits),
                Spread:  Math.Round((this.Ask - this.Bid) * Math.Pow(10,this.Digits),0),
                TickCounter: this.TickCounter,
                TradingConnectionQuotes: ConvertToMDRoutingItems(TradinConnectionsQuotes, this.Ask, this.Bid)
            );
            // foreach (var item in msg.TradingConnectionQuotes)
            // {
            //     Console.WriteLine(item);
            // }
            return msg;
        }

    public static SubMsgMDRoutingItem[] ConvertToMDRoutingItems(List<TCSymbolQuote> symbolQuotes, double fastAsk, double fastBid)
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
            AskOffset: 0, 
            BidOffset: 0, 
            AskAfterOffset: sq.Ask, 
            BidAfterOffset: sq.Bid,
            BuyGap: Math.Round((fastBid - sq.Ask)* Math.Pow(10,sq.Digits),0),
            SellGap: Math.Round((sq.Bid - fastAsk)* Math.Pow(10,sq.Digits),0),
            IsBuyGap: (fastBid - sq.Ask) > 0, // Set to true if BuyGap > 0
            IsSellGap: (sq.Bid - fastAsk) > 0, // Set to true if SellGap > 0
            TickCounter: sq.TickNumber
        )).ToArray();
    }

    }
}
