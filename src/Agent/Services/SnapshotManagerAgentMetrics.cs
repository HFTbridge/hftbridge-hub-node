using HFTbridge.TCLIB;
using HFTbridge.Msg;
using HFTbridge.Node.Agent;

namespace HFTbridge.Agent.Services
{
    public class SnapshotManagerAgentMetrics
    {
        private readonly HFTBridgeEngine _engine;
        private readonly EventGateway _eventGateway;
        private readonly HardwareMetricsManager _hardware;
        private readonly GeoLocationManager _geo;
        private readonly string _organizationId;
        private readonly string _nodeId;

        public SnapshotManagerAgentMetrics(
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

        public MsgSnapshotSingleAgentMetrics GetSnapshot()
        {
            var geoData = _geo.Data;
            var locParts = geoData.loc?.Split(',') ?? new string[] { "0", "0" };
            double.TryParse(locParts[0], out var latitude);
            double.TryParse(locParts[1], out var longitude);

            var snapshot = new MsgSnapshotSingleAgentMetrics(
                //Ts: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Ts: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                OrganizationId: _organizationId,
                NodeId: _nodeId,
                OperatingSystem: _hardware.OperatingSystem,
                HardwareCPU: _hardware.HardwareCPU,
                HardwareNC: _hardware.HardwareNC,
                HardwareGPU: _hardware.HardwareGPU,
                ProcessedRequests: 0,
                ProcessedTicks: 0,
                ProcessedSignals: 0,
                ProcessedTrades: 0,
                CountTradingConnections: 0,
                CountStreamingSymbols: 0,
                TickToTradeAbove1Ms: 0,
                TickToTradeBelow1Ms: 0,
                TickToTradeAverageMs: 0
            );

            return snapshot;
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