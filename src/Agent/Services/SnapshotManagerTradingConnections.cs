using HFTbridge.TCLIB;
using HFTbridge.Msg;
using HFTbridge.Node.Agent;

namespace HFTbridge.Agent.Services
{
    public class SnapshotManagerTradingConnections
    {
        private readonly HFTBridgeEngine _engine;
        private readonly EventGateway _eventGateway;
        private readonly HardwareMetricsManager _hardware;
        private readonly GeoLocationManager _geo;
        private readonly string _organizationId;
        private readonly string _nodeId;

        public SnapshotManagerTradingConnections(
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

        public MsgSnapshotSingleAgentTradingConnections GetSnapshot()
        {
            var connections = new List<SubMsgSnapshotSingleAgentTradingConnectionsItem>();
            foreach (var item in _engine.TradingConnections.Values)
            {
                var ownerDetails = item.GetTcAccountOwnerDetails();
                var record = new SubMsgSnapshotSingleAgentTradingConnectionsItem()
                {
                    OrganizationId = ownerDetails.OrganizationId,
                    TradingAccountId = ownerDetails.TradingAccountId,
                    ConnectionStatus = item.GetConnectionStatus().ToString(),
                    //ConnectionStatus = "Connected",
                    Balance = 0,
                    Equity = 0,
                    PingMs = item.PingMs,
                    StreamingSymbols = 0,
                    TpsAccount = 0,
                    TpmAccount = 0,
                    OpenedTradesCount = 0,
                    ErrorCount = 0,
                    ConnectedAtTs = DateTime.UtcNow.Ticks,
                    BrokerName = item.BrokerWithGeoLocation.Name,
                    Company = item.BrokerWithGeoLocation.Company,
                    IconUrl = item.BrokerWithGeoLocation.IconUrl,
                    Host = item.BrokerWithGeoLocation.GeoLocations[0].host,
                    Port = item.BrokerWithGeoLocation.GeoLocations[0].port,
                    CountryCode = item.BrokerWithGeoLocation.GeoLocations[0].countryCode,
                    Region = item.BrokerWithGeoLocation.GeoLocations[0].region,
                    RegionName = item.BrokerWithGeoLocation.GeoLocations[0].regionName,
                    Zip = item.BrokerWithGeoLocation.GeoLocations[0].zip,
                    Lat = item.BrokerWithGeoLocation.GeoLocations[0].lat,
                    Lng = item.BrokerWithGeoLocation.GeoLocations[0].lng,
                    Timezone = item.BrokerWithGeoLocation.GeoLocations[0].timezone,
                    Isp = item.BrokerWithGeoLocation.GeoLocations[0].isp,
                    Org = item.BrokerWithGeoLocation.GeoLocations[0].org,
                    Query = item.BrokerWithGeoLocation.GeoLocations[0].query,
                };
                connections.Add(record);
            }

            var snapshot = new MsgSnapshotSingleAgentTradingConnections(
                //Ts: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Ts: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                OrganizationId: _organizationId,
                NodeId: _nodeId,
                OperatingSystem: _hardware.OperatingSystem,
                CountryCode: _geo.Data.country,
                ConnectedAccounts: connections
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