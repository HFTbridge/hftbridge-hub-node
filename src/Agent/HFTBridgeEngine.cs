using HFTbridge.Msg;
using HFTbridge.TCLIB;
using HFTbridge.Agent.Services;
using HFTbridge.Agent;
using Disruptor;
using Disruptor.Dsl;


namespace HFTbridge.Node.Agent
{
    public class HFTBridgeEngine
    {
        private readonly Dictionary<string,SubMsgSnapshotFullSingleAgentNodeTC> _tradingConnections;
        private readonly TCFactory _tc;
        private readonly FastFeedManager _fastFeed;

        private readonly Disruptor<FastMarketDataEvent> _fastFeedAnalysisPipe;

        public event Action<MsgMDRoutingBulk> OnMsgMDRoutingBulk;


        public HFTBridgeEngine()
        {
            _tradingConnections = new Dictionary<string, SubMsgSnapshotFullSingleAgentNodeTC>();
            _tc = new TCFactory();

            _fastFeedAnalysisPipe = new Disruptor<FastMarketDataEvent>(() => 
                new FastMarketDataEvent(), ringBufferSize: 262144);

            _fastFeedAnalysisPipe.HandleEventsWith(
                    new AggregateMarketDataHandler() // Thread 1:  GET SYMBOLS from ALL Connected Accounts
                )
                .Then(new TransformMarketDataHandler()) // Thread 2:  CALCULATE GAPS
                .Then(new StrategyMarketDataHandler()) // Thread 3: RUN STRATEGIES
                .Then(new PublishMarketDataHandler(this)) // Thread 4: PUBLISH TO INFRASTRUCTUR
                .Then(new OffsetMarketDataHandler()); // Thread 5: UPDATE OFFSET LOOKUP STORE

            _fastFeedAnalysisPipe.Start();

            _fastFeed = new FastFeedManager(_fastFeedAnalysisPipe);

            
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

            var adapter = _tc.CreateTCAdapter(msg.TradingAccountProvider, msg.TradingAccountConnectionString, msg.BrokerId);
            adapter.Connect();
            Console.WriteLine(adapter.Config.Version);
            _tradingConnections[msg.TradingAccountId] = record;

            
        }


        public void Disconnect( MsgStopTradingAccountRequest msg, string organizationId, string userId)
        {
            _tradingConnections.Remove(msg.TradingAccountId);
        }

        public List<SubMsgSnapshotFullSingleAgentNodeTC> GetConnectionRecords()
        {
            return _tradingConnections.Values.ToList();
        }

        public void InvokeOnMsgMDRoutingBulk(MsgMDRoutingBulk msg)
        {
            OnMsgMDRoutingBulk?.Invoke(msg);
        }
        
    }
}