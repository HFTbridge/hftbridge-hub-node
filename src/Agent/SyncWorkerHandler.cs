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
        }
    }
}