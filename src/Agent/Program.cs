using HFTbridge.Node.Shared.Services;
using HFTbridge.Msg;
using HFTbridge.TCLIB;

namespace HFTbridge.Node.Agent
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var organizationId = "PUBLIC";
            var nodeId = "MIKE-DEV";

            var engine = new HFTBridgeEngine();
            
            var mothershipService = new MothershipService("https://hub.agent.hft-app.net", nodeId, organizationId, new SyncWorkerHandler(organizationId, nodeId, engine));

         
            var handler = new MsgHandler(mothershipService._eventGateway, engine);
            mothershipService.Start();


            while (true)
            {
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