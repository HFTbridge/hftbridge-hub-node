namespace Agent.Models
{
    public class LowLatencyMarketDataRouting
    {
        public DateTime Timestamp {get;set;}

        public string SymbolRouting {get;set;}
        public string MainFeedID {get;set;}

        public double MainAsk {get;set;}
        public double MainBid {get;set;}
        public double MainAverage {get;set;}
        public double MainSpread {get;set;}

        public long RoutingUpdateCounter {get;set;}
        public long MainFeedUpdateCounter {get;set;}

        public int Digits {get;set;}
        public double DigitsMultiplier {get;set;}

        public Dictionary<string,LowLatencyMarketDataSymbolState> SlowFeeds {get;set;}



        public LowLatencyMarketDataRouting(string mainFeedID, string symbolRouting, int digits )
        {
            MainFeedID = mainFeedID;
            SymbolRouting = symbolRouting;
            Digits = digits;
            DigitsMultiplier = Math.Pow(10,digits);
            SlowFeeds = new Dictionary<string, LowLatencyMarketDataSymbolState>();
        }

        public void Update(LowLatencyTickDataEvent data)
        {
            Timestamp = data.IncomingTickTs;
            if (MainFeedID == data.FeedNameMapping)
            {
                MainAsk = Math.Round(data.Ask,Digits);
                MainBid = Math.Round(data.Bid,Digits);
                MainAverage = Math.Round(((data.Ask + data.Bid)/2),Digits);
                MainSpread = Math.Round(((data.Ask - data.Bid)*DigitsMultiplier),0);
                foreach (var item in SlowFeeds.Values)
                {
                    item.Update(data.Ask, data.Bid, Timestamp);
                }
                MainFeedUpdateCounter ++;
            }
            else
            {
                var slowFeedKey = data.GetMatchingSymbolID();
                if (SlowFeeds.TryGetValue(slowFeedKey, out var routingItem))
                {
                    routingItem.Update(data, MainAsk, MainBid, Timestamp);
                }
                else
                {
                    SlowFeeds[slowFeedKey] = new LowLatencyMarketDataSymbolState(data, MainAsk, MainBid, Digits,Timestamp );
                }
            }
            RoutingUpdateCounter++;

            data.Publish(ToMsg());
            
        }

        private SubMsgMDRoutingItem[] GetSlowFeeds()
        {
            return SlowFeeds.Values.Select(sq => new SubMsgMDRoutingItem(
                Ts: sq.Timestamp.Ticks,
                OrganizationId: "BueNnVbKOVhy0TYkMAZ9",
                TradingAccountId: sq.FeedID,
                SymbolKey: sq.SymbolKey,
                SymbolRouting: this.SymbolRouting,
                Digits: this.Digits,
                Ask: sq.Ask,
                Bid: sq.Bid,
                AveragePrice: sq.AverageAfterOffset,
                Spread: sq.Spread,
                AskOffset: sq.OffsetAsk, 
                BidOffset: sq.OffsetBid, 
                AskAfterOffset: sq.AskAfterOffset, 
                BidAfterOffset: sq.BidAfterOffset,
                BuyGapSimpleAdjustSpread: sq.BuyGapSimple,
                SellGapSimpleAdjustSpread: sq.SellGapSimple,
                BuyGapAverage: sq.BuyGapAverage,
                SellGapAverage: sq.SellGapAverage,
                BuyGapSpreadCorrect : sq.BuyGapMinusSpread,
                SellGapSpreadCorrect: sq.SellGapMinusSpread,
                BuyGapSpreadCorrectAdjustSpread: sq.BuyGapMinusSpread,
                SellGapSpreadCorrectAdjustSpread: sq.SellGapMinusSpread,
                IsBuyGap: sq.BuyGapMinusSpread > 0, // Set to true if BuyGap > 0
                IsSellGap: sq.SellGapMinusSpread > 0, // Set to true if SellGap > 0
                //  IsBuyGap: sq.IsBuyGap, // Set to true if BuyGap > 0
                // IsSellGap: sq.IsSellGap, // Set to true if SellGap > 0
                TickCounter: sq.TickCounter
            )).ToArray();
        }



        public SubMsgMDRouting ToMsg()
        {
            var ts = DateTime.UtcNow;
            var msg = new SubMsgMDRouting(
                IncomingTickTs: this.Timestamp.Ticks,
                ProcessedTickTs: ts.Ticks,
                ProcessingMs: (ts - Timestamp).TotalMilliseconds,
                SymbolKey: this.SymbolRouting,
                SymbolRouting: this.SymbolRouting,
                Digits: this.Digits,
                Ask: this.MainAsk,
                Bid: this.MainBid,
                AveragePrice: this.MainAverage,
                Spread:  this.MainSpread,
                TickCounter: this.RoutingUpdateCounter,
                TradingConnectionQuotes: GetSlowFeeds(),
                IsAnalysisRunning: true
                //TradingConnectionQuotes: ConvertToMDRoutingItems(TradinConnectionsQuotes, this.Ask, this.Bid)
            );
            // foreach (var item in msg.TradingConnectionQuotes)
            // {
            //     Console.WriteLine(item);
            // }
            return msg;
        }




    }

    public class LowLatencyMarketDataSymbolState
    {
        // mapping
        public string SymbolKeyMatchingId {get;set;}

        public string FeedID {get;set;}
        public string SymbolKey {get;set;}

        // Prices
        public double Ask {get;set;}
        public double Bid {get;set;}
        public double Average {get;set;}
        public double Spread {get;set;}

        // Offset Prices
        public double AskAfterOffset {get;set;}
        public double BidAfterOffset {get;set;}
        public double AverageAfterOffset {get;set;}
        public double SpreadAfterOffset {get;set;}

        // Timestamps
        public DateTime Timestamp {get;set;}
        public long TickCounter {get;set;}

        // Offset calculation
        public double OffsetAsk {get;set;}
        public double OffsetBid {get;set;}
        public int OffsetCalculationCounter {get;set;}
        public double OffsetCalculationSumAsk{get;set;}
        public double OffsetCalculationSumBid{get;set;}
        public DateTime LastOffsetCalcTs {get;set;}


        // Gaps
        public double BuyGapSimple {get;set;}
        public double SellGapSimple {get;set;}
        public double BuyGapMinusSpread {get;set;}
        public double SellGapMinusSpread {get;set;}
        public double BuyGapAverage {get;set;}
        public double SellGapAverage {get;set;}
        public double BuyGapAverageMinusSpread {get;set;}
        public double SellGapAverageMinusSpread {get;set;}
        public bool IsBuyGap {get;set;}
        public bool IsSellGap {get;set;}
        public DateTime LastGapSignalFound {get;set;}

        // Digits
        public int Digits {get;set;}
        public double DigitsMultiplier {get;set;}



        public LowLatencyMarketDataSymbolState(LowLatencyTickDataEvent data, double fastAsk, double fastBid, int digits, DateTime incomingTs)
        {
            IsBuyGap = false;
            IsSellGap = false;
            LastGapSignalFound = DateTime.UtcNow;
            // Timestamp
            Timestamp = incomingTs;
            // LastOffsetCalcTs = DateTime.UtcNow;
            // OffsetCalculationWarmup = 0;
            // MAPPING
            SymbolKeyMatchingId = data.GetMatchingSymbolID();
            FeedID = data.FeedNameMapping;
            SymbolKey = data.FeedSymbolKeyMapping;

            // Digits
            Digits = digits;
            DigitsMultiplier = Math.Pow(10,digits);

            // Initial Prices
            Ask = data.Ask;
            Bid = data.Bid;
            Average = Math.Round(((data.Ask + data.Bid)-(data.Ask - data.Bid))/2,digits);
            Spread = Math.Round((data.Ask - data.Bid)*DigitsMultiplier,0);

            // Calculate base offsets
            OffsetAsk = fastAsk - data.Ask;
            OffsetBid = fastBid - data.Bid;
            CalculateOffsetValues(fastAsk,fastBid,Timestamp);

            // Calculate Offset Prices
            AskAfterOffset = data.Ask + OffsetAsk;
            BidAfterOffset = data.Bid + OffsetBid;
            AverageAfterOffset = Math.Round((AskAfterOffset + BidAfterOffset)/2,digits);
            SpreadAfterOffset = Math.Round((AskAfterOffset - BidAfterOffset)*DigitsMultiplier,0);

            CalculateGaps(fastAsk,fastBid);

        }

        public void CalculateOffsetValues(double fastAsk, double fastBid, DateTime incomingTs)
        {   
            OffsetCalculationSumAsk = OffsetCalculationSumAsk + (fastAsk - Ask);
            OffsetCalculationSumBid = OffsetCalculationSumBid + (fastBid - Bid);
            OffsetCalculationCounter ++;

            if ((incomingTs - LastOffsetCalcTs).TotalMinutes >= 2)
            {
                OffsetAsk = OffsetCalculationSumAsk/OffsetCalculationCounter;
                OffsetBid = OffsetCalculationSumBid/OffsetCalculationCounter;
                OffsetCalculationSumAsk = 0;
                OffsetCalculationSumBid = 0;
                OffsetCalculationCounter = 0;
                LastOffsetCalcTs = DateTime.UtcNow;
            }
        }

        public void Update(LowLatencyTickDataEvent data, double fastAsk, double fastBid, DateTime incomingTs)
        {
            // Timestamp
            Timestamp = incomingTs;

             // Initial Prices
            Ask = data.Ask;
            Bid = data.Bid;
            Average = Math.Round((data.Ask + data.Bid)/2,Digits);
            Spread = Math.Round((data.Ask - data.Bid)*DigitsMultiplier,0);

           

             // Calculate Offset Prices
            AskAfterOffset = data.Ask + OffsetAsk;
            BidAfterOffset = data.Bid + OffsetBid;
            AverageAfterOffset = Math.Round(((AskAfterOffset + BidAfterOffset)-(data.Ask - data.Bid))/2,Digits);
            SpreadAfterOffset = Math.Round((AskAfterOffset - BidAfterOffset)*DigitsMultiplier,0);

            CalculateOffsetValues(fastAsk,fastBid,Timestamp);

            // Calculate Gaps
            CalculateGaps(fastAsk,fastBid);

          

            TickCounter++;
        }

        public void Update(double fastAsk, double fastBid, DateTime incomingTs)
        {
            // Timestamp
            Timestamp = incomingTs;
            CalculateOffsetValues(fastAsk,fastBid,Timestamp);
            CalculateGaps(fastAsk,fastBid);
        }

        public void CalculateGaps(double fastAsk, double fastBid)
        {
            BuyGapSimple = Math.Round((fastBid - AskAfterOffset)*DigitsMultiplier,0);
            SellGapSimple = Math.Round((BidAfterOffset - fastAsk)*DigitsMultiplier,0);
            BuyGapMinusSpread = BuyGapSimple - Spread;
            SellGapMinusSpread = SellGapSimple - Spread;
            BuyGapAverage = Math.Round(((((fastAsk + fastBid)/2)  - AverageAfterOffset)*DigitsMultiplier),0) ;
            SellGapAverage =  Math.Round(((AverageAfterOffset - ((fastAsk + fastBid)/2))*DigitsMultiplier),0) ;
            BuyGapAverageMinusSpread = BuyGapAverage - Spread;
            SellGapAverageMinusSpread = SellGapAverage - Spread;


                if (BuyGapMinusSpread > 1)
                {
                    IsBuyGap = true;
                    IsSellGap = false;
                   // LastGapSignalFound = Timestamp;
                }
                else if (SellGapMinusSpread > 1)
                {
                    //LastGapSignalFound = Timestamp;
                    IsBuyGap = false;
                    IsSellGap = true;
                }
                else
                {
                    IsBuyGap = false;
                    IsSellGap = false;
                }

            
        }

        public override string ToString()
        {
            return $"UTC {Timestamp:HH:mm:ss.ffffff}|{SymbolKeyMatchingId}|Spread:{Spread}|Average:{Average}|<G: B:{BuyGapSimple}|S:{SellGapSimple}> <GS: B:{BuyGapMinusSpread}|S:{SellGapMinusSpread}> <GA: B:{BuyGapAverage}|S:{SellGapAverage}> <GSA: B:{BuyGapAverageMinusSpread}|S:{SellGapAverageMinusSpread}>";
        }




    }
}