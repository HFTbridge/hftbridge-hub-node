using Newtonsoft.Json;
using HFTbridge.Msg;
using Disruptor;
using Disruptor.Dsl;
using HFTbridge.Node.Agent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agent;


namespace HFTbridge.Agent
{
    public class LowLatencyPublishMarketDataHandler : IEventHandler<LowLatencyTickDataEvent>
    {
        private readonly HFTBridgeEngine _engine;
        private readonly List<SubMsgMDRouting> _buffer = new List<SubMsgMDRouting>();
        private readonly object _lock = new object();
        private readonly Timer _timer;
        private readonly string _nodeId;

        public LowLatencyPublishMarketDataHandler(HFTBridgeEngine engine)
        {
            _engine = engine;
            _nodeId = AgentConfig.NodeId;
            // Timer that triggers every second
            _timer = new Timer(OnTimerElapsed, null, 1000, 1000); // 1000 ms = 1 second
        }

        public async void OnEvent(LowLatencyTickDataEvent data, long sequence, bool endOfBatch)
        {
            try
            {
                lock (_lock)
                {
                    if (!data.ToBeProcessed)
                    {
                        Console.WriteLine("ERROR WITH BUFFER OVERFLOW!!!!!");
                        return;
                    }
                    //var test = data.Msg;
                    _buffer.Add(data.Msg);
                    data.ToBeProcessed = false;

                   
                   // Console.WriteLine($"tc: {data.Msg.TradingConnectionQuotes.Count()} | FAST FEED COUNT: {data.Msg.}");
                    // Publish if buffer size exceeds 500
                    // if ("EURUSD" == data.Msg.SymbolRouting)
                    // {
                    //     Console.WriteLine($"TICK: {data.Msg.TickCounter}");
                    // }
                    //data.Msg = null;
                   
                    
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
                NodeId = _nodeId
            };

            // Console.WriteLine(Program.NodeId);

            _buffer.Clear();
            _engine.InvokeOnMsgMDRoutingBulk(msg);
           Console.WriteLine($"--- NEW STREAM ---- Published {msg.Ticks.Count()}");
        }

        // Call this method to stop the timer when no longer needed
        public void Stop()
        {
            _timer.Dispose();
        }

       
    }
}
