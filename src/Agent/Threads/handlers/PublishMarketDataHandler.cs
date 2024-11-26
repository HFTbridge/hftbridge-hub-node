using Newtonsoft.Json;
using HFTbridge.Msg;
using Disruptor;
using Disruptor.Dsl;
using HFTbridge.Node.Agent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace HFTbridge.Agent
{
    public class PublishMarketDataHandler : IEventHandler<FastMarketDataEvent>
    {
        private readonly HFTBridgeEngine _engine;
        private readonly List<SubMsgMDRouting> _buffer = new List<SubMsgMDRouting>();
        private readonly object _lock = new object();
        private readonly Timer _timer;

        public PublishMarketDataHandler(HFTBridgeEngine engine)
        {
            _engine = engine;
            // Timer that triggers every second
            _timer = new Timer(OnTimerElapsed, null, 1000, 1000); // 1000 ms = 1 second
        }

        public async void OnEvent(FastMarketDataEvent data, long sequence, bool endOfBatch)
        {
            try
            {
                lock (_lock)
                {
                    _buffer.Add(data.ToMsg());

                    // Publish if buffer size exceeds 500
                    if (_buffer.Count >= 500)
                    {
                        Publish();
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void OnTimerElapsed(object state)
        {
            lock (_lock)
            {
                if (_buffer.Any())
                {
                    Publish();
                }
            }
        }

        public void Publish()
        {
            if (!_buffer.Any()) return;

            var msg = new MsgMDRoutingBulk()
            {
                Ts = DateTime.UtcNow.Ticks,
                Ticks = _buffer.ToArray(),
                NodeId = Program.NodeId
            };

            //Console.WriteLine(Program.NodeId);

            _buffer.Clear();
            _engine.InvokeOnMsgMDRoutingBulk(msg);
           Console.WriteLine($"--- Published {msg.Ticks.Count()}");
        }

        // Call this method to stop the timer when no longer needed
        public void Stop()
        {
            _timer.Dispose();
        }

       
    }
}
