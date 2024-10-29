using HFTbridge.Node.Shared;

using HFTbridge.Msg;

namespace HFTbridge.Node.Agent
{
    public class SyncWorkerHandler : ISyncWorkerHandler
    {
        private readonly string _organizationId;
        private readonly string _nodeId;
        private readonly HFTBridgeEngine _engine;

        public SyncWorkerHandler(string organizationId, string nodeId, HFTBridgeEngine engine )
        {
            _engine = engine;
            _organizationId = organizationId;
            _nodeId = nodeId;
        }
        public void OnEverySecond(EventGateway eventGateway, string os, string countryCode)
        {

            // Create dummy data
            var connectedAccounts = _engine.GetConnectionRecords();
        

            var snapshot = new MsgSnapshotFullSingleAgentNode(
                Ts: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                OrganizationId: _organizationId,
                NodeId: _nodeId,
                OperatingSystem: os,
                CountryCode: countryCode,
                ConnectedAccounts: connectedAccounts
            );

            eventGateway.Send(snapshot, 
                organizationId:_organizationId,
                severity:"Debug",
                nodeId:_nodeId
            );

            eventGateway.Send(_engine.MDStore.GetSnapshot(), 
                organizationId:_organizationId,
                severity:"Debug",
                nodeId:_nodeId
            );

            eventGateway.Send(GetMsgSnapshotSingleAgentInformation(), 
                organizationId:_organizationId,
                severity:"Debug",
                nodeId:_nodeId
            );

            eventGateway.Send(GetMsgSnapshotSingleAgentLiveTrades(), 
                organizationId:_organizationId,
                severity:"Debug",
                nodeId:_nodeId
            );
        }


        public MsgSnapshotSingleAgentInformation GetMsgSnapshotSingleAgentInformation()
        {
            var agentInfo = new MsgSnapshotSingleAgentInformation(
                Ts: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                OrganizationId: "Org987",
                NodeId: "NodeABC123",
                OperatingSystem: "Windows 10",
                CountryCode: "US",
                TpsTotal: 1200,
                TickToTradeMax: 100,
                TickToTradeMin: 10,
                TickToTradeAverage: 55,
                ConnectedTradingAccountsCount: 350,
                StreamingMarketDataSymbolsCount: 75
            );

        return agentInfo;
        }

        public MsgSnapshotSingleAgentLiveTrades GetMsgSnapshotSingleAgentLiveTrades()
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
                    TradeId: "Trade321",
                    TradeTicket: "Ticket098",
                    Direction: "Sell",
                    SymbolKey: "USDJPY",
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
                    TradeId: "Trade321",
                    TradeTicket: "Ticket098",
                    Direction: "Sell",
                    SymbolKey: "USDJPY",
                    Digits: 3,
                    OpenPrice: 110.123,
                    PnL: -75.25,
                    TP: 109.000,
                    SL: 111.000
                )
            };

            // Create the main dummy object
            var dummyMsgSnapshot = new MsgSnapshotSingleAgentLiveTrades(
                Ts: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                TotalPnL: 75.50,
                TradesCount: dummyTrades.Count,
                NodeId: "Mike-Dev",
                Trades: dummyTrades
            );

            return dummyMsgSnapshot;
        }

        public MsgSnapshotSingleAgentMarketData GetMsgSnapshotSingleAgentMarketData()
        {
            // Create a list of dummy quotes
            var quotes = new List<SubMsgSnapshotSingleAgentMarketDataQuote>
            {
                new SubMsgSnapshotSingleAgentMarketDataQuote(
                    Ts: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    TickNumber: 123456,
                    OrganizationId: "PUBLIC",
                    TradingAccountId: "Acc456",
                    SymbolKey: "EURUSD.pro",
                    SymbolRouting: "EURUSD",
                    Digits: 5,
                    Ask: 1.12345,
                    Bid: 1.12300,
                    AveragePrice: 1.12322,
                    Spread: 0.00045,
                    Tps: 10,
                    Tpm: 600,
                    Delta1M: 0.0002,
                    Delta5M: 0.0005
                ),
                new SubMsgSnapshotSingleAgentMarketDataQuote(
                    Ts: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    TickNumber: 123356,
                    OrganizationId: "PUBLIC",
                    TradingAccountId: "Acc456",
                    SymbolKey: ".FUS100",
                    SymbolRouting: "NDX",
                    Digits: 5,
                    Ask: 1.12345,
                    Bid: 1.12300,
                    AveragePrice: 1.12322,
                    Spread: 0.00045,
                    Tps: 10,
                    Tpm: 600,
                    Delta1M: 0.0002,
                    Delta5M: 0.0005
                ),
                new SubMsgSnapshotSingleAgentMarketDataQuote(
                    Ts: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    TickNumber: 789012,
                    OrganizationId: "Org789",
                    TradingAccountId: "Acc987",
                    SymbolKey: "GBP/USD",
                    SymbolRouting: "GBPUSD",
                    Digits: 4,
                    Ask: 1.2345,
                    Bid: 1.2340,
                    AveragePrice: 1.2342,
                    Spread: 0.0005,
                    Tps: 8,
                    Tpm: 480,
                    Delta1M: 0.0003,
                    Delta5M: 0.0006
                )
            };

            // Create the main market data snapshot
            var marketData = new MsgSnapshotSingleAgentMarketData(
                Ts: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                TotalTps: 18,
                SymbolsCount: 3,
                NodeId: "PUBLIC",
                Quotes: quotes
            );

            return marketData;
        }
    }
}