using HFTbridge.TCLIB;
using HFTbridge.Msg;
using HFTbridge.Node.Agent;

namespace HFTbridge.Agent.Services
{
    public class SnapshotManagerAgentStatus
    {
        private readonly HFTBridgeEngine _engine;
        private readonly EventGateway _eventGateway;
        private readonly HardwareMetricsManager _hardware;
        private readonly GeoLocationManager _geo;
        private readonly string _organizationId;
        private readonly string _nodeId;

        public SnapshotManagerAgentStatus(
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

        public MsgSnapshotSingleAgentStatus GetSnapshot()
        {
            var geoData = _geo.Data;
            var locParts = geoData.loc?.Split(',') ?? new string[] { "0", "0" };
            double.TryParse(locParts[0], out var latitude);
            double.TryParse(locParts[1], out var longitude);

            var snapshot = new MsgSnapshotSingleAgentStatus(
                Ts: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                // Ts: DateTimeOffset.UtcNow.Ticks,
                OrganizationId: _organizationId,
                NodeId: _nodeId,
                OperatingSystem: _hardware.OperatingSystem,
                HardwareCPU: _hardware.HardwareCPU,
                HardwareNC: _hardware.HardwareNC,
                HardwareGPU: _hardware.HardwareGPU,
                Country: geoData.country,
                CountryCode: geoData.country,
                City: geoData.city,
                Region: geoData.region,
                RegionName: geoData.region, // Adjust if RegionName differs from `region`
                Zip: geoData.postal,
                Lat: latitude,
                Lon: longitude,
                Timezone: geoData.timezone,
                Isp: "N/A",
                OrgServer: geoData.org,
                QueryDNS: "N/A",
                OriginIp: geoData.ip
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