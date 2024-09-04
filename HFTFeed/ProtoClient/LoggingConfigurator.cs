using Serilog;
using Serilog.Formatting.Compact;

namespace ProtoClient;

public static class LoggingConfigurator
{
    public static void ConfigureLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(new CompactJsonFormatter(), "log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
