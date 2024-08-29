using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading;
using HFTbridge.Msg;

namespace HFTbridge.Node.Shared.Services
{
    public class EventGatewayConfiguration : IEventGatewayConfiguration
    {
        public string ServiceName {get;}
        public string ServiceVersion {get;}
        public string ServiceSharedVersion {get;}

        public EventGatewayConfiguration(string name, string version, string sharedVersion)
        {
            ServiceName = name;
            ServiceVersion= sharedVersion;
            ServiceSharedVersion= sharedVersion;
        }
    }


    public class MothershipService : IMsgPublisher
    {

        public long MsgTimestamp {get;}
        public long MsgCounter {get;}


        private string Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
        private int SchemaVersion = MsgSchema.Version;
        private string SharedNodeVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
        private string NODE_ORGANIZATION_ID = "PUBLIC";
        private string NODE_SECRET = "B396A51A5D3EDA7CBB2B7FF8A662F";
        private string NODE_ID = "CRYPTO";
        private string URL_SERVICE = "https://hub.dc.hft-app.net";
        private string URL_FORMAT = "{0}/node-mothership?nodeId={1}&version={2}&schemaVersion={3}&sharedNodeVersion={4}&secret={5}&organizationId={6}&countryCode={7}&os={8}";
        private string MothershipURL;

        public EventGateway _eventGateway {get;set;}

        private HubConnection _agentHubClient;

        private readonly GeoLocationService _geoLocationService;
        private readonly MachineInformationService _machineInformationService;
        private readonly SyncWorker _syncWorker;
        private readonly ISyncWorkerHandler _syncWorkerHandler;

        public MothershipService(string url, string nodeId, string organizationId, ISyncWorkerHandler syncWorkerHandler)
        {
            ConfigureLogger();
            _geoLocationService = new GeoLocationService();
            _machineInformationService = new MachineInformationService();
            _eventGateway = new EventGateway(this, new EventGatewayConfiguration("AGENT-HUB", Version, SharedNodeVersion ));

            _syncWorkerHandler = syncWorkerHandler;
            _syncWorker = new SyncWorker(_eventGateway, syncWorkerHandler);

            NODE_ORGANIZATION_ID = organizationId;
            NODE_ID = nodeId;
            URL_SERVICE = url;
            MothershipURL = $"{string.Format(URL_FORMAT, URL_SERVICE, nodeId, Version, SchemaVersion, SharedNodeVersion, NODE_SECRET, NODE_ORGANIZATION_ID, _geoLocationService.Data.country, _machineInformationService.Data.OperatingSystem.Name)}";


        }

        public async void Publish(RabbitMsgWrapper msg)
        {

             await _agentHubClient.SendAsync("HandleClientEvent", msg);

        }

        public async void Start()
        {
           

            _agentHubClient = BuildHubClient(MothershipURL);

            // // Attach the event handler
            _agentHubClient.On<RabbitMsgWrapper>("OnRabbitMsgWrapper", (msg) =>
            {
                ReceiveMessage(msg);
            });

            await _agentHubClient.StartAsync();

            await _agentHubClient.SendAsync("UpdateNodeNetworkInformation", _geoLocationService.Data);
            await _agentHubClient.SendAsync("UpdateNodeMachineInformation", _machineInformationService.GetHardwareInformationJson());

            // Keep the program running
            Log.Logger.Information("< --- HFTbridge Data Center is running --- >");
          
        }

        private void ReceiveMessage(RabbitMsgWrapper msg)
        {
            Log.Logger.Information("Received: {messageType}", msg.MessageType);

            _eventGateway.Receive(msg);
        }


        private async Task ReconnectAgent(HubConnection hubClient)
        {
            var timeout = 0;
            while (hubClient.State != HubConnectionState.Connected)
            {
                try
                {
                    await hubClient.StartAsync();
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Reconnection attempt failed: {message}", ex.Message);
                }

                await Task.Delay(1000);
                timeout++;
                Log.Logger.Warning($"SERIOUS SIGNAL R TIMEOUT : {timeout}");
                if (timeout > 10)
                {
                    Log.Logger.Error("SERIOUS SIGNAL R TIMEOUT FINAL !");
                    return;
                }
            }
        }


        private HubConnection BuildHubClient(string url)
        {
            Console.WriteLine(url);
            var hubClient = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                })
                .AddMessagePackProtocol()
                .Build();

            hubClient.Closed += async (error) =>
            {
                Log.Logger.Error("SERIOUS SIGNAL DISCONNECT {error}", error.Message);
                await ReconnectAgent(hubClient);
            };

            // Optional: Hook into the Reconnected event if you want to know when a reconnection happens
            hubClient.Reconnected += (connectionId) =>
            {
                Log.Logger.Information("Reconnected with connection ID: {connectionId}", connectionId);
                PublishNetowrkAndMachine();
                return Task.CompletedTask;
            };
            return hubClient;
        }

        private async void PublishNetowrkAndMachine()
        {
            await _agentHubClient.SendAsync("UpdateNodeNetworkInformation", _geoLocationService.Data);
            await _agentHubClient.SendAsync("UpdateNodeMachineInformation", _machineInformationService.GetHardwareInformationJson());

        }



            private void ConfigureLogger()
        {
            var loggerConfiguration = new LoggerConfiguration();

#if DEBUG
            loggerConfiguration
                .MinimumLevel.Debug() // Set minimum log level to Debug for verbose logging
                .WriteTo.Console();   // Write logs to console in debug mode
#else
            loggerConfiguration
                .MinimumLevel.Information() // Set minimum log level to Information in release mode
                .WriteTo.File($"hftbridge-dc-{NODE_ID}-log-.txt", rollingInterval: RollingInterval.Day); // Write logs to file in release mode
#endif

            Log.Logger = loggerConfiguration.CreateLogger();
            Log.Logger.Debug(" <--- Starting DC LOGGER --->");
        }

    }
}