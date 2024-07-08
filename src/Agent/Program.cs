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
    private const string NODE_SECRET_KEY = "7645";
    private const string NODE_ID = "fewfweg";
    private const string URL_AGENT_HUB_FORMAT = "https://hub.agent.hft-app.net/node-mothership?agentId=1";
    // private const string URL_HUB = "http://localhost:5242";

    static async Task Main(string[] args)
    {
        // // 1. Load Config
        ConfigureLogger();
        ConfigReader.LoadConfiguration();

        // // 2. Build Engine and Request Handler
        // var engine = new HftBridgeEngine();
        // var messageReceiveService = new ReceiveMessagesService(Log.Logger, engine);

        // // 3. Build SignalR Client
        var agentHubClient = BuildHubClient(URL_AGENT_HUB_FORMAT);

        // // 4. Build SignalR event publisher/sender 
        // var agentEventHandler = new AgentEventHandler(Log.Logger, engine, agentHubClient, messageReceiveService);

        // 5. Start SignalR stream
        await agentHubClient.StartAsync();

        // 6. Keep the program running
        Log.Logger.Information("< --- HFTbridge Agent is running --- >");
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
        Log.Logger.Debug(" <--- Starting HFTbridge Agent --->");
    }


    private static HubConnection BuildHubClient(string urlFormat)
    {
        var hubClient = new HubConnectionBuilder()
            .WithUrl(URL_AGENT_HUB_FORMAT, options =>
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



