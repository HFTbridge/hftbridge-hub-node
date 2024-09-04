using Disruptor;
using Disruptor.Dsl;

namespace HFTbridge.Agent
{
    public class PublishMarketDataHandler : IEventHandler<FastMarketDataEvent>
    {
      //  private readonly EventGateway _eventGateway;

        public PublishMarketDataHandler()
        {
          //  _eventGateway = eventGateway;
        }

        public async void OnEvent(FastMarketDataEvent data, long sequence, bool endOfBatch)
        {
            try
            {
                data.CalculateProcessingTimeMs();
                Console.WriteLine($"Latency: {data.ProcessingMs}ms");
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

    }
}