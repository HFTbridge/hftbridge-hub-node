using Disruptor;
using Disruptor.Dsl;
using HFTbridge.Node.Agent;

namespace HFTbridge.Agent
{
    public class AggregateMarketDataHandler : IEventHandler<FastMarketDataEvent>
    {
        
        private readonly HFTBridgeEngine _engine;

        public AggregateMarketDataHandler(HFTBridgeEngine engine)
        {
            _engine = engine;
        }

        public async void OnEvent(FastMarketDataEvent data, long sequence, bool endOfBatch)
        {
            try
            {
                foreach (var item in _engine.TradingConnections.Values)
                {
                    
                    var quotes = item.GetSymbolsQuotesByRouting(data.SymbolRouting);
                    foreach (var item2 in quotes)
                    {
                        data.TradinConnectionsQuotes.Add(item2);
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

    }
}