using Disruptor;
using Disruptor.Dsl;
using Agent.Models;

namespace HFTbridge.Agent
{
    public class UpdateMarketDataStoreHandler : IEventHandler<FastMarketDataEvent>
    {
        private readonly StoreMarketData _mdStore;
        public UpdateMarketDataStoreHandler(StoreMarketData mdStore)
        {
            _mdStore = mdStore;
        }

        public async void OnEvent(FastMarketDataEvent data, long sequence, bool endOfBatch)
        {
            try
            {
                _mdStore.Update("INTERNAL.SPOT",data.SymbolKey,data.Ask,data.Bid);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

    }
}