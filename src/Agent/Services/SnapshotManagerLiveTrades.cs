using HFTbridge.TCLIB;
using HFTbridge.Msg;
using HFTbridge.Node.Agent;

namespace HFTbridge.Agent.Services
{
    public class SnapshotManagerLiveTrades
    {
        private readonly HFTBridgeEngine _engine;
        private readonly EventGateway _eventGateway;
        private readonly HardwareMetricsManager _hardware;
        private readonly GeoLocationManager _geo;
        private readonly string _organizationId;
        private readonly string _nodeId;

        public SnapshotManagerLiveTrades(
            HFTBridgeEngine engine, 
            EventGateway eventGateway, 
            string organizationId, 
            string nodeId,
            HardwareMetricsManager hardware,
            GeoLocationManager geo
        )
        {
            _engine = engine;
            _eventGateway = eventGateway;
            _organizationId = organizationId;
            _nodeId = nodeId;
            _hardware = hardware;
            _geo = geo;
        }

        public MsgSnapshotSingleAgentLiveTrades GetSnapshot()
        {
            // Create dummy list of trades
            var dummyTrades = new List<SubMsgSnapshotSingleAgentLiveTradesItem>
            {
                new SubMsgSnapshotSingleAgentLiveTradesItem(
                    TradeOpenedTs: 1634065600000,
                    OrganizationId: "BueNnVbKOVhy0TYkMAZ9",
                    TradingAccountId: "0f0bbd45-6e4b-454d-ad70-fa3121a47d04",
                    TradeId: "Trade789",
                    TradeTicket: "Ticket012",
                    Direction: "Buy",
                    SymbolKey: "EURUSD",
                    Digits: 5,
                    OpenPrice: 1.12345,
                    PnL: 150.75,
                    TP: 1.13000,
                    SL: 1.12000
                ),
                new SubMsgSnapshotSingleAgentLiveTradesItem(
                    TradeOpenedTs: 1634079200000,
                    OrganizationId: "BueNnVbKOVhy0TYkMAZ9",
                    TradingAccountId: "0f0bbd45-6e4b-454d-ad70-fa3121a47d04",
                    TradeId: "654433hgd43hg",
                    TradeTicket: "Ticket098",
                    Direction: "Sell",
                    SymbolKey: "XAUUSD",
                    Digits: 3,
                    OpenPrice: 110.123,
                    PnL: -75.25,
                    TP: 109.000,
                    SL: 111.000
                ),
                new SubMsgSnapshotSingleAgentLiveTradesItem(
                    TradeOpenedTs: 1634079200000,
                    OrganizationId: "BueNnVbKOVhy0TYkMAZ9",
                    TradingAccountId: "0f0bbd45-6e4b-454d-ad70-fa3121a47d04",
                    TradeId: "Trade3263431",
                    TradeTicket: "Ticket098",
                    Direction: "Sell",
                    SymbolKey: "NDX",
                    Digits: 3,
                    OpenPrice: 110.123,
                    PnL: -15.25,
                    TP: 109.000,
                    SL: 111.000
                )
            };

            // Create the main dummy object
            var dummyMsgSnapshot = new MsgSnapshotSingleAgentLiveTrades(
               // Ts: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Ts: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                TotalPnL: 75.50,
                TradesCount: dummyTrades.Count,
                NodeId: _nodeId,
                Trades: dummyTrades
            );

            return dummyMsgSnapshot;
        }


        public void SendSnapshot()
        {
            _eventGateway.Send(GetSnapshot(), 
                organizationId: _organizationId,
                severity: "Debug",
                nodeId: _nodeId
            );
        }



    }
}