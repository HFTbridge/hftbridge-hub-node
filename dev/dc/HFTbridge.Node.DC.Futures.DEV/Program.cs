using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;  // Add this using directive
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace HFTbridge.Agent;
class Program
{   
    private static string Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
    private static string SchemaVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
    private static string SharedNodeVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
    private const string NODE_ORGANIZATION_ID = "PUBLIC";
    private const string NODE_SECRET = "B396A51A5D3EDA7CBB2B7FF8A662F";
    private const string NODE_ID = "FUTURES";
    private const string URL_SERVICE = "https://hub.dc.hft-app.net";
    private const string URL_FORMAT = "{0}/node-mothership?id={1}&version={2}&schemaVersion={3}&shareNodeVersion={4}&secret={5}&organizationId={6}";
    // private const string URL_HUB = "http://localhost:5242";

    private static string MothershipURL = $"{string.Format(URL_FORMAT, URL_SERVICE, NODE_ID, Version, SchemaVersion, SharedNodeVersion, NODE_SECRET, NODE_ORGANIZATION_ID)}";

    static async Task Main(string[] args)
    {
        ConfigureLogger();

        var agentHubClient = BuildHubClient(URL_FORMAT);
        await agentHubClient.StartAsync();

        // 6. Keep the program running
        Log.Logger.Information("< --- HFTbridge Data Center is running --- >");
        while (true)
        {
            await Task.Delay(1000);
        }
    }

    private static void ConfigureLogger()
    {
        var loggerConfiguration = new LoggerConfiguration();

        #if DEBUG
        loggerConfiguration
            .MinimumLevel.Debug() // Set minimum log level to Debug for verbose logging
            .WriteTo.Console();   // Write logs to console in debug mode
        #else
        loggerConfiguration
            .MinimumLevel.Information() // Set minimum log level to Information in release mode
            .WriteTo.File("hftbridge-agent-log-.txt", rollingInterval: RollingInterval.Day); // Write logs to file in release mode
        #endif

        Log.Logger = loggerConfiguration.CreateLogger();
        Log.Logger.Debug(" <--- Starting DC LOGGERt --->");
    }


    private static HubConnection BuildHubClient(string urlFormat)
    {
        var hubClient = new HubConnectionBuilder()
            .WithUrl(MothershipURL, options =>
            {
                options.Transports = HttpTransportType.WebSockets;
            })
            // .ConfigureLogging(logging => {
            //     // Bind serilog here logging
            //     logging.AddSerilog(); // Add this line
            // })
            .AddMessagePackProtocol() 
            .Build();

        hubClient.Closed += async (error) =>
        {
            Log.Logger.Error("SERIOUS SIGNAL DISCONNECT {error}", error.Message);
            ReconnectAgent(hubClient);
        };
        return hubClient;
    }

    private static async void ReconnectAgent(HubConnection hubClient)
    {
        var timeout = 0;
        while (hubClient.State != HubConnectionState.Connected)
        {
            await hubClient.StartAsync();
            Thread.Sleep(1000);
            timeout ++;
            Log.Logger.Warning($"SERIOUS SIGNAL R TIMEOUT : {timeout}");
            if(timeout > 10)
            {
                Log.Logger.Error("SERIOUS SIGNAL R TIMEOUT FINAL !");
                return;
            }

        }
    }
    
}



