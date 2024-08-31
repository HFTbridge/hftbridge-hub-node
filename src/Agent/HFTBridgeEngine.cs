using HFTbridge.Msg;

namespace HFTbridge.Node.Agent
{
    public class HFTBridgeEngine
    {
        private readonly Dictionary<string,SubMsgSnapshotFullSingleAgentNodeTC> _tradingConnections;
        public HFTBridgeEngine()
        {
            _tradingConnections = new Dictionary<string, SubMsgSnapshotFullSingleAgentNodeTC>();
        }

        public void Connect(MsgStartTradingAccount msg, string organizationId, string userId)
        {
            var record = new SubMsgSnapshotFullSingleAgentNodeTC()
            {
                OrganizationId = organizationId,
                TradingAccountId = msg.TradingAccountId,
                ConnectionStatus = "Connected",
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
            _tradingConnections[msg.TradingAccountId]=record;
        }

        public void Disconnect( MsgStopTradingAccount msg, string organizationId, string userId)
        {
            _tradingConnections.Remove(msg.TradingAccountId);
        }

        public List<SubMsgSnapshotFullSingleAgentNodeTC> GetConnectionRecords()
        {
            return _tradingConnections.Values.ToList();
        }

    }
}