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
    public class MothershipService
    {
        private string Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
        private int SchemaVersion = MsgSchema.Version;
        private string SharedNodeVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
        private string NODE_ORGANIZATION_ID = "PUBLIC";
        private string NODE_SECRET = "B396A51A5D3EDA7CBB2B7FF8A662F";
        private string NODE_ID = "CRYPTO";
        private string URL_SERVICE = "https://hub.dc.hft-app.net";
        private string URL_FORMAT = "{0}/node-mothership?nodeId={1}&version={2}&schemaVersion={3}&sharedNodeVersion={4}&secret={5}&organizationId={6}";
        private string MothershipURL;

        public MothershipService(string url, string nodeId, string organizationId)
        {
            NODE_ORGANIZATION_ID = organizationId;
            NODE_ID = nodeId;
            URL_SERVICE = url;
            MothershipURL = $"{string.Format(URL_FORMAT, URL_SERVICE, NODE_ID, Version, SchemaVersion, SharedNodeVersion, NODE_SECRET, NODE_ORGANIZATION_ID)}";
            ConfigureLogger();
        }

        public async void Start()
        {
           

            var agentHubClient = BuildHubClient(MothershipURL);

            // // Attach the event handler
            // agentHubClient.On<long, string, string, string, string, byte[], string, string, string>("HandleNewMessage", (ts, severity, actionId, exchange, messageType, @event, userId, nodeId, mail) =>
            // {
            //     ReceiveMessage(ts, severity, actionId, exchange, messageType, @event, userId, nodeId, mail);
            // });

            await agentHubClient.StartAsync();

            // Keep the program running
            Log.Logger.Information("< --- HFTbridge Data Center is running --- >");
          
        }

        private static void ReceiveMessage(long ts, string severity, string actionId, string exchange, string messageType, byte[] @event, string userId, string nodeId, string mail)
        {
            Log.Logger.Information("Received: {messageType}", messageType);
        }


        private static async Task ReconnectAgent(HubConnection hubClient)
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


        private static HubConnection BuildHubClient(string url)
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
            return hubClient;
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