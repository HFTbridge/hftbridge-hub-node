namespace HFTbridge.Agent;
public static class ConfigReader
{
    public static string HubHost;
    public static string AgentSecretKey;
    public static string AgentId;

    public static bool LogDisplayHeartbeat;
    public static bool LogDisplayMarketData;
    public static bool LogDisplayLatencyGapTick;
    public static bool LogDisplayLatencyGapOffset;

// test2

    public static void LoadConfiguration()
    {
        // Path to the configuration file in the program's directory
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HFTbridgeAgentConfig.json");

        if (!File.Exists(configPath))
            throw new InvalidOperationException("Config file not found!");

        var json = File.ReadAllText(configPath);

        // Parse the JSON data
        using JsonDocument doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        HubHost = root.GetProperty("HubHost").GetString();
        AgentSecretKey = root.GetProperty("AgentSecretKey").GetString();
        AgentId = root.GetProperty("AgentId").GetString();

        LogDisplayHeartbeat = Convert.ToBoolean(root.GetProperty("LogDisplayHeartbeat").GetBoolean());
        LogDisplayMarketData = Convert.ToBoolean(root.GetProperty("LogDisplayMarketData").GetBoolean());
        LogDisplayLatencyGapTick = Convert.ToBoolean(root.GetProperty("LogDisplayLatencyGapTick").GetBoolean());
        LogDisplayLatencyGapOffset = Convert.ToBoolean(root.GetProperty("LogDisplayLatencyGapOffset").GetBoolean());
        
    }

}