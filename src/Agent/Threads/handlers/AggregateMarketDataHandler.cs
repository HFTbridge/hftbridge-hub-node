using Disruptor;
using Disruptor.Dsl;

namespace HFTbridge.Agent
{
    public class AggregateMarketDataHandler : IEventHandler<FastMarketDataEvent>
    {

        public AggregateMarketDataHandler()
        {

        }

        public async void OnEvent(FastMarketDataEvent data, long sequence, bool endOfBatch)
        {
            try
            {
                
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

    }
}