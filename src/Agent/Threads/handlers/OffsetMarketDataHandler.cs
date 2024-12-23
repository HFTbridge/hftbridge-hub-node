using Disruptor;
using Disruptor.Dsl;

namespace HFTbridge.Agent
{
    public class OffsetMarketDataHandler : IEventHandler<FastMarketDataEvent>
    {

        public OffsetMarketDataHandler()
        {

        }

        public async void OnEvent(FastMarketDataEvent data, long sequence, bool endOfBatch)
        {
            try
            {
                data.TradinConnectionsQuotes.Clear();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

    }
}