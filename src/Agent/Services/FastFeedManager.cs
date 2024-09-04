using System.Text;
using ProtoClient;
using com.chronoxor.simple;
using Disruptor;
using Disruptor.Dsl;

namespace HFTbridge.Agent.Services
{
    public class FastFeedManager
    {

        private readonly SimpleProtoClient _feed;

        private readonly Dictionary<string,int> _digits;

        private readonly StringBuilder _keyBuilder;

        private readonly string _tradingAccount;
        private readonly Disruptor<FastMarketDataEvent> _fastFeedAnalysisPipe;

        public FastFeedManager(Disruptor<FastMarketDataEvent> fastFeedAnalysisPipe)
        {
            _fastFeedAnalysisPipe = fastFeedAnalysisPipe;
            _digits = new Dictionary<string, int>();
            AddSymbols();
            _feed = new SimpleProtoClient("207.167.96.187", 9009);
            _feed.ReceivedNotify_NewMdNotify += HandleNewTick;
            _keyBuilder = new StringBuilder();
            _tradingAccount = "INTERNAL.SPOT";
        }

        public void Start()
        {
          
            _feed.ConnectAndStart();
        }

        public void AddSymbols()
        {
            _digits["XBR/USD"] = 2;
            _digits["XTI/USD"] = 2;
            _digits["XAG/USD"] = 2;
            _digits["XAU/USD"] = 2;

            _digits["J225"] = 2;
            _digits["GDAXI"] = 2;
            _digits["NDX"] = 2;
            _digits["SPX"] = 2;
            _digits["WS30"] = 1;

            _digits["CAD/JPY"] = 3;
            _digits["USD/JPY"] = 3;
            _digits["GBP/JPY"] = 3;
            _digits["AUD/JPY"] = 3;
            _digits["CHF/JPY"] = 3;
            _digits["EUR/JPY"] = 3;

            _digits["AUD/NZD"] = 5;
            _digits["NZD/USD"] = 5;
            _digits["USD/CHF"] = 5;
            _digits["USD/CAD"] = 5;
            _digits["GBP/USD"] = 5;
            _digits["GBP/CHF"] = 5;
            _digits["GBP/CAD"] = 5;
            _digits["EUR/USD"] = 5;
            _digits["EUR/GBP"] = 5;
            _digits["EUR/CHF"] = 5;
            _digits["EUR/CAD"] = 5;
            _digits["EUR/AUD"] = 5;
            _digits["AUD/USD"] = 5;
        }

        private void HandleNewTick(NewMdNotify notify)
        {

            if (!_digits.ContainsKey(notify.symbolkey))
            {
                return;
            }

            using (var scope = _fastFeedAnalysisPipe.PublishEvent())
            {
                try
                {
                    var data = scope.Event();
                    data.Publish(notify.symbolkey, notify.symbolkey, notify.ask, notify.bid, _digits[notify.symbolkey]);
                }
                catch (System.Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }


    }
}