namespace Agent
{
    public static class AgentConfig
    {
        public static string NodeId;
        public static string OrganizationId;
        public static string FastFeedHost;
        public static int FastFeedPort;
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

            NodeId = root.GetProperty("NodeId").GetString();
            OrganizationId = root.GetProperty("OrganizationId").GetString();
            FastFeedHost = root.GetProperty("FastFeedHost").GetString();
            FastFeedPort = root.GetProperty("FastFeedPort").GetInt32();
        }
    }
}