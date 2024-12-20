using HFTbridge.Msg;
using HFTbridge.TCLIB;
using HFTbridge.Agent.Services;
using HFTbridge.Agent;
using Agent.Models;
using Disruptor;
using Disruptor.Dsl;

using HFTbridge.TC.Shared;

namespace HFTbridge.Node.Agent
{
    public class HFTBridgeEngine
    {
        private readonly Dictionary<string,SubMsgSnapshotFullSingleAgentNodeTC> _tradingConnections;

        public readonly Dictionary<string,ITCAdapter> TradingConnections;
        private readonly TCFactory _tc;
        private readonly FastFeedManager _fastFeed;

        public readonly StoreMarketData MDStore;

        private readonly Disruptor<FastMarketDataEvent> _fastFeedAnalysisPipe;
        private readonly Disruptor<LowLatencyTickDataEvent> _lowLatencyMDPath;

        public event Action<MsgMDRoutingBulk> OnMsgMDRoutingBulk;
        public event Action<MsgTCLog, string, string> OnMsgTCLog;
        


        public HFTBridgeEngine()
        {
       

            TradingConnections = new Dictionary<string, ITCAdapter>();

            MDStore = new StoreMarketData(TradingConnections);

            _tradingConnections = new Dictionary<string, SubMsgSnapshotFullSingleAgentNodeTC>();
            _tc = new TCFactory();

            _lowLatencyMDPath = new Disruptor<LowLatencyTickDataEvent>(() => 
                new LowLatencyTickDataEvent(), ringBufferSize: 4096);

            _lowLatencyMDPath.HandleEventsWith(
                    new LowLatencyTickUpdateHandler() // Thread 1:  GET SYMBOLS from ALL Connected Accounts
                )
                .Then(new LowLatencyPublishMarketDataHandler(this));

            _lowLatencyMDPath.Start();
            

            _fastFeedAnalysisPipe = new Disruptor<FastMarketDataEvent>(() => 
                new FastMarketDataEvent(), ringBufferSize: 262144);

            _fastFeedAnalysisPipe.HandleEventsWith(
                    new AggregateMarketDataHandler(this) // Thread 1:  GET SYMBOLS from ALL Connected Accounts
                )
                .Then(new UpdateMarketDataStoreHandler(MDStore))
                .Then(new TransformMarketDataHandler()) // Thread 2:  CALCULATE GAPS
                .Then(new StrategyMarketDataHandler()) // Thread 3: RUN STRATEGIES
               // .Then(new PublishMarketDataHandler(this)) // Thread 4: PUBLISH TO INFRASTRUCTUR
                .Then(new OffsetMarketDataHandler()); // Thread 5: UPDATE OFFSET LOOKUP STORE

            _fastFeedAnalysisPipe.Start();

            _fastFeed = new FastFeedManager(_fastFeedAnalysisPipe, MDStore, _lowLatencyMDPath);

            
            _fastFeed.Start();
        }

        public void Connect(MsgStartTradingAccountRequest msg, string organizationId, string userId)
        {
            if (_tradingConnections.ContainsKey(msg.TradingAccountId))
            {
                throw new InvalidOperationException($"A connection with TradingAccountId {msg.TradingAccountId} already exists.");
            }

            var record = new SubMsgSnapshotFullSingleAgentNodeTC()
            {
                OrganizationId = organizationId,
                TradingAccountId = msg.TradingAccountId,
                ConnectionStatus = "Connecting",
                Balance = 0,
                Equity = 0,
                PingMs = 0,
                StreamingSymbols = 0,
                TpsAccount = 0,
                TpmAccount = 0,
                OpenedTradesCount = 0,
                ErrorCount = 0,
                ConnectedAtTs = DateTime.UtcNow.Ticks
            };

            var adapter = _tc.CreateTCAdapter(
                msg.TradingAccountProvider, 
                msg.TradingAccountConnectionString, 
                msg.BrokerId, 
                msg.TradingAccountId,
                organizationId, 
                userId,
                true
                );

                Console.WriteLine(msg.ToString());

         //   Console.WriteLine("TEST === " +  msg.TradingAccountId + "|" + organizationId);
            //var adapter = _tc.CreateTCAdapter(msg.TradingAccountProvider, msg.TradingAccountConnectionString, msg.BrokerId);



            // ADD LOGGING
            adapter.TCLogger.OnLogEntry += logEntry =>{
                
                
                var msgLog = new MsgTCLog()
                {
                    Ts = logEntry.Ts,
                    TradingAccountId = msg.TradingAccountId,
                    Severity = (int)logEntry.Severity,
                    Message = logEntry.Message
                };
                OnMsgTCLog?.Invoke(msgLog, organizationId, userId);

                return;
            };

            adapter.OnTickValue += logTick =>{
                // Console.WriteLine(logTick.Symbolkey + "|" + logTick.SymbolRouting + "|" + logTick.AvgPrice);

                // return;
                using (var scope = _lowLatencyMDPath.PublishEvent())
                {
                    var data = scope.Event();
                    data.Publish(logTick.FeedID, logTick.SymbolRouting, logTick.SymbolKey, logTick.Ask, logTick.Bid);
                }
            };

            // CONNECT
            adapter.Connect();

            foreach (var item in msg.Subscribe)
            {
                adapter.Subscribe(item.SymbolKey, item.SymbolRouting, item.Digits);
            }

            // adapter.Subscribe("EURUSD", "EURUSD", 5);
            // adapter.Subscribe("GBPUSD", "GBPUSD", 5);
            
            
            _tradingConnections[msg.TradingAccountId] = record;
            TradingConnections[msg.TradingAccountId] = adapter;

            
        }


        public void Disconnect( MsgStopTradingAccountRequest msg, string organizationId, string userId)
        {
            _tradingConnections.Remove(msg.TradingAccountId);
            var adapter = TradingConnections[msg.TradingAccountId];
            adapter.Disconnect();
            TradingConnections.Remove(msg.TradingAccountId);

        }

        public List<SubMsgSnapshotFullSingleAgentNodeTC> GetConnectionRecords()
        {
            return _tradingConnections.Values.ToList();
        }

        public List<SubMsgSnapshotFullSingleAgentNodeTC> GetConnections()
        {
            return _tradingConnections.Values.ToList();
        }

        public void InvokeOnMsgMDRoutingBulk(MsgMDRoutingBulk msg)
        {
            OnMsgMDRoutingBulk?.Invoke(msg);
        }
        
    }
}