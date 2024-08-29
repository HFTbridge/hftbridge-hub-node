using HFTbridge.Node.Shared;

using HFTbridge.Msg;

namespace HFTbridge.Node.Agent
{
    public class SyncWorkerHandler : ISyncWorkerHandler
    {
        public void OnEverySecond(EventGateway eventGateway)
        {
            // Create dummy data
            var connectedAccounts = new List<SubMsgSnapshotFullSingleAgentNodeTC>
            {
                new SubMsgSnapshotFullSingleAgentNodeTC(
                    OrganizationId: "Org123",
                    TradingAccountId: "TAC001",
                    ConnectionStatus: "Active",
                    Balance: 1500.75,
                    Equity: 1600.50,
                    PingMs: 23.4,
                    StreamingSymbols: 50,
                    TpsAccount: 200,
                    TpmAccount: 500,
                    OpenedTradesCount: 10,
                    ErrorCount: 2,
                    ConnectedAtTs: DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                ),
                new SubMsgSnapshotFullSingleAgentNodeTC(
                    OrganizationId: "Org123",
                    TradingAccountId: "TAC002",
                    ConnectionStatus: "Inactive",
                    Balance: 200.00,
                    Equity: 300.00,
                    PingMs: 50.1,
                    StreamingSymbols: 30,
                    TpsAccount: 150,
                    TpmAccount: 300,
                    OpenedTradesCount: 5,
                    ErrorCount: 1,
                    ConnectedAtTs: DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3600 // 1 hour ago
                )
            };
        

            var snapshot = new MsgSnapshotFullSingleAgentNode(
                Ts: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                OrganizationId: "Org123",
                NodeId: "Node456",
                OperatingSystem: "Windows 10",
                CountryCode: "US",
                ConnectedAccounts: connectedAccounts
            );

            eventGateway.Send(snapshot, 
                organizationId:"Public",
                severity:"Debug",
                nodeId:"MIKE-DEV"
            );
        }
    }
}