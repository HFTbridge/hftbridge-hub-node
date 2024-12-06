using Disruptor;
using Disruptor.Dsl;
using HFTbridge.Node.Agent;

namespace HFTbridge.Agent
{
    public class LowLatencyTickUpdateHandler : IEventHandler<LowLatencyTickDataEvent>
    {
        private LowLatencyMarketDataStore _mdStore;

        public LowLatencyTickUpdateHandler()
        {
            _mdStore = new LowLatencyMarketDataStore();
        }

        public void OnEvent(LowLatencyTickDataEvent data, long sequence, bool endOfBatch)
        {
            //Console.WriteLine(data.ToString());
            //_mdStore.Update(data.FeedNameMapping , data.FeedSymbolRoutingMapping , data.FeedSymbolKeyMapping, data.Ask, data.Bid);
            _mdStore.Update(data);
            // data.GlobalProcessedTickCounter = sequence;
            // Console.WriteLine(sequence);

        }

    }
}