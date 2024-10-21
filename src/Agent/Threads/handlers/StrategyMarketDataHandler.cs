using Disruptor;
using Disruptor.Dsl;

namespace HFTbridge.Agent
{
    public class StrategyMarketDataHandler : IEventHandler<FastMarketDataEvent>
    {

        public StrategyMarketDataHandler()
        {

        }

        public async void OnEvent(FastMarketDataEvent data, long sequence, bool endOfBatch)
        {
            try
            {
                data.CalculateProcessingTimeMs();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

    }
}