using HFTbridge.TCLIB;
using HFTbridge.Msg;
using HFTbridge.Node.Agent;
using System.Collections.Generic;
using System.Linq;

namespace HFTbridge.Agent.Services
{
    public class SnapshotManagerMarketData
    {
        private readonly HFTBridgeEngine _engine;
        private readonly EventGateway _eventGateway;
        private readonly HardwareMetricsManager _hardware;
        private readonly GeoLocationManager _geo;
        private readonly string _organizationId;
        private readonly string _nodeId;

        private readonly Queue<MsgSnapshotSingleAgentMarketData> _snapshotHistory = new();
        private readonly int _maxSnapshots = 60;


        public SnapshotManagerMarketData(
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



        public MsgSnapshotSingleAgentMarketData GetSnapshot()
        {
            // Placeholder for snapshot retrieval logic
            return _engine.MDStore.GetSnapshot();
        }

        public void SendSnapshot()
        {
            var currentSnapshot = GetSnapshot();

            // Maintain a rolling list of the last 60 snapshots
            if (_snapshotHistory.Count >= _maxSnapshots)
            {
                _snapshotHistory.Dequeue(); // Remove the oldest snapshot
            }
            

            if (_snapshotHistory.Count > 1)
            {
                var oldestSnapshot = _snapshotHistory.First();
                var newestSnapshot = _snapshotHistory.Last();

                for (int i = 0; i < currentSnapshot.Quotes.Count; i++)
                {
                    var currentQuote = currentSnapshot.Quotes[i];
                    var matchingQuoteOldest = oldestSnapshot.Quotes.FirstOrDefault(q => q.SymbolKey == currentQuote.SymbolKey);
                    var matchingQuoteNewest = newestSnapshot.Quotes.FirstOrDefault(q => q.SymbolKey == currentQuote.SymbolKey);

                    if (matchingQuoteNewest != null)
                    {
                        // Calculate TPS: difference between current and newest
                        currentQuote = currentQuote with { Tps = (int)(currentQuote.TickNumber - matchingQuoteNewest.TickNumber) };
                    }

                    if (matchingQuoteOldest != null)
                    {
                        // Calculate TPM: difference between current and oldest
                        int tpm = (int)(currentQuote.TickNumber - matchingQuoteOldest.TickNumber);
                        double delta1M = currentQuote.AveragePrice - matchingQuoteOldest.AveragePrice;

                        // Adjust Delta1M based on Digits
                        double multiplier = Math.Pow(10, currentQuote.Digits);
                        delta1M *= multiplier;

                        currentQuote = currentQuote with
                        {
                            Tpm = tpm,
                            Delta1M = Math.Round(delta1M,0)
                        };
                    }

                    currentSnapshot.Quotes[i] = currentQuote;
                }
            }

            // Send the updated snapshot
            _eventGateway.Send(
                currentSnapshot,
                organizationId: _organizationId,
                severity: "Debug",
                nodeId: _nodeId
            );

            _snapshotHistory.Enqueue(currentSnapshot); // Add the current snapshot
        }


    }
}
