using HFTbridge.Node.Shared.Services;
using HFTbridge.Msg;
using HFTbridge.TCLIB;
using HFTbridge.Agent.Services;

namespace HFTbridge.Node.Agent
{
    class Program
    {
        public static string NodeId = "Manoj-Agent-LMAX-AVG";
    //    public static string NodeId = "Mike-Agent";
    //   public static string NodeId = "Team-Agent-LMAX";
        // public static string NodeId = "DEV-LD4";
        static async Task Main(string[] args)
        {
            var organizationId = "PUBLIC";
            // var nodeId = "DEV-LD4-1";
            var nodeId = NodeId;

            var hardware = new HardwareMetricsManager();
            var geo = new GeoLocationManager();
            

            var engine = new HFTBridgeEngine();
            
            var mothershipService = new MothershipService("https://hub.agent.hft-app.net", nodeId, organizationId, new SyncWorkerHandler(organizationId, nodeId, engine));

            var handler = new MsgHandler(mothershipService._eventGateway, engine, nodeId, organizationId);
            mothershipService.Start();

            // Snapshot Manager
            var snapshotTC = new SnapshotManagerTradingConnections(
                engine,
                mothershipService._eventGateway,
                organizationId,
                nodeId,
                hardware,
                geo
            );

            var snapshotAgent= new SnapshotManagerAgentStatus(
                engine,
                mothershipService._eventGateway,
                organizationId,
                nodeId,
                hardware,
                geo
            );

            var snapshotTrades = new SnapshotManagerLiveTrades(
                engine,
                mothershipService._eventGateway,
                organizationId,
                nodeId,
                hardware,
                geo
            );

            var snapshotAgentMetrics = new SnapshotManagerAgentMetrics(
                engine,
                mothershipService._eventGateway,
                organizationId,
                nodeId,
                hardware,
                geo
            );

            var snapshotMD = new SnapshotManagerMarketData(
                engine,
                mothershipService._eventGateway,
                organizationId,
                nodeId,
                hardware,
                geo
            );

            

            while (true)
            {
                snapshotTC.SendSnapshot();
                snapshotAgent.SendSnapshot();
                snapshotTrades.SendSnapshot();
                snapshotAgentMetrics.SendSnapshot();
                snapshotMD.SendSnapshot();
                // Send Snapshot Agent Details
                // Send Snapshot TC
                // Send Snapshot TC Symbols MD Level 1
                // Send Snapshot TC Symbols Routing
                // Send Snapshot TC Live Trades

                await Task.Delay(1000);
            }
        }

    }
}






// [FIXProtocolMessage]
// public record struct SendTradeFIXMSG(
//     [TAG: 53] long Ts,
//     [TAG: 43]  string Severity,
//     [TAG: 15]  string ActionId,
//     [TAG: 16]  string Exchange,
//     [TAG: 85]  string MessageType,
//     [TAG: 11]  string ByteMsg,
//     [TAG: 53] string OrganizationId,
// );


// byte[] rawMsg = FIXSChema.Serialize(new SendTradeFIXMSG());
// FIXEngine.Send(rawMsg);