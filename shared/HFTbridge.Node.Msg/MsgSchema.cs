
// MsgSchema.cs
global using MessagePack;
global using RabbitMQ.Client.Events;
global using HFTbridge.Msg;
using System.Text;

namespace HFTbridge.Msg;

public static class MsgSchema
{
    public static int Version = 26;

//--------------------------------------------------------------------------------------------------------------------------------------------
//------[ DECODE MESSAGE FROM RABBIT MQ LIBRARAY  ]
//--------------------------------------------------------------------------------------------------------------------------------------------
    public static RabbitMsgWrapper FromRabbit(BasicDeliverEventArgs args)
    {
        if (args.BasicProperties == null || args.BasicProperties.Headers == null)
            throw new ArgumentNullException("BasicProperties or Headers are null");

        string ActionId = null, AppName = null, AppVersion = null, MessageType = null;
        string OrganizationId = null, SchemaVersion = null,  Severity = null, SharedVersion = null;
        string UserEmail = null, UserId = null, NodeId = null;
        string Exchange = args.Exchange;

        long Ts = 0;
        // int SchemaVersion = 0;
        string testTs = null;

        ExtractHeader(args, "ActionId", value => ActionId = value);
        ExtractHeader(args, "AppName", value => AppName = value);
        ExtractHeader(args, "AppVersion", value => AppVersion = value);
        ExtractHeader(args, "MessageType", value => MessageType = value);
        ExtractHeader(args, "OrganizationId", value => OrganizationId = value);
        ExtractHeader(args, "SchemaVersion", value => SchemaVersion = value);
        ExtractHeader(args, "Severity", value => Severity = value);
        ExtractHeader(args, "SharedVersion", value => SharedVersion = value);
        ExtractHeader(args, "UserEmail", value => UserEmail = value);
        ExtractHeader(args, "UserId", value => UserId = value);
        ExtractHeader(args, "NodeId", value => NodeId = value);

        // Example usage
        try
        {
            Ts = ExtractHeaderAsLong(args, "Ts");
        }
        catch (HeaderException ex)
        {
            Console.WriteLine($"Header {ex.Header} is missing.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }



        return new RabbitMsgWrapper(
            Ts: Ts,
            Severity: Severity,
            ActionId: ActionId,
            Exchange: Exchange,
            MessageType: MessageType,
            ByteMsg: args.Body.ToArray(),
            OrganizationId: OrganizationId,
            UserId: UserId,
            NodeId: NodeId,
            UserEmail: UserEmail,
            AppName: AppName,
            AppVersion: AppVersion,
            SharedVersion: SharedVersion,
            SchemaVersion: SchemaVersion
        );
    }

    private static void ExtractHeader(BasicDeliverEventArgs args, string headerName, Action<string> assignValue)
    {
        if (args.BasicProperties.Headers.TryGetValue(headerName, out var headerBytes))
        {
            var headerValue = Encoding.UTF8.GetString((byte[])headerBytes);
            assignValue(headerValue);
        }
    }

    private static long ExtractHeaderAsLong(BasicDeliverEventArgs args, string headerName)
    {
        if (args.BasicProperties.Headers == null)
        {
            throw new ArgumentNullException(nameof(args.BasicProperties.Headers), "Headers are null");
        }

        if (args.BasicProperties.Headers.TryGetValue(headerName, out var headerValue))
        {
            switch (headerValue)
            {
                case byte[] byteArray:
                    var stringValue = Encoding.ASCII.GetString(byteArray);
                    if (long.TryParse(stringValue, out var longValueFromBytes))
                    {
                        return longValueFromBytes;
                    }
                    throw new InvalidCastException($"Unable to convert header {headerName} value from byte array to long. Value: {stringValue}");
                
                case long longValue:
                    return longValue;

                case int intValue:
                    return intValue;
                    
                case string strValue:
                    if (long.TryParse(strValue, out var longValueFromString))
                    {
                        return longValueFromString;
                    }
                    throw new InvalidCastException($"Unable to convert header {headerName} value from string to long. Value: {strValue}");
                    
                default:
                    throw new InvalidCastException($"Unsupported header type for {headerName}: {headerValue.GetType()}");
            }
        }
        throw new HeaderException { Header = headerName };
    }

    

}

public class HeaderException : Exception
{
    public string Header { get; set; }
}



//--------------------------------------------------------------------------------------------------------------------------------------------
//------[ RABBIT MQ INTERNAL WRAPPER TO BE SENT OVER AND RECEIVED FROM RABBIT MQ  ]
//--------------------------------------------------------------------------------------------------------------------------------------------
[MessagePackObject]
public record struct RabbitMsgWrapper(
    // WHEN ?
    [property: Key(0)] long Ts,
    // WHERE ?
    [property: Key(1)] string Severity,
    [property: Key(2)] string ActionId,
    [property: Key(3)] string Exchange,
    // WHAT ?
    [property: Key(4)] string MessageType,
    [property: Key(5)] byte[] ByteMsg,
    // WHO ?
    [property: Key(6)] string OrganizationId,
    [property: Key(7)] string UserId,
    [property: Key(8)] string NodeId,
    [property: Key(9)] string UserEmail,
    // ENV
    [property: Key(10)] string AppName,
    [property: Key(11)] string AppVersion,
    [property: Key(12)] string SharedVersion,
    [property: Key(13)] string SchemaVersion
);

//--------------------------------------------------------------------------------------------------------------------------------------------
//------[ DICTIONARY TO LOOKUP EXCHANGE NAME FOR EACH MESSAGE  ]
//--------------------------------------------------------------------------------------------------------------------------------------------

// SHOULD BE LIKE THIS : TODO! Please write in JS the function to generate this dictionary
// public static readonly Dictionary<string, string> ExchangeMessageLookup = new Dictionary<string, string>
// {
//     { "exchange.agent.md", "MsgMDRoutingBulk" },
//     { "exchange.snapshot.notification", "MsgSnapshotSingleAgentStatus" },
//     { "exchange.snapshot.notification", "MsgSnapshotSingleAgentTradingConnections" },
//     { "exchange.snapshot.notification", "MsgSnapshotSingleAgentMarketData" },
//     { "exchange.snapshot.notification", "MsgSnapshotSingleAgentLiveTrades" },
//     { "exchange.snapshot.notification", "MsgSnapshotSingleAgentMetrics" }
// };



//--------------------------------------------------------------------------------------------------------------------------------------------
//------[ DEFINITIONS OF ALL INTERNAL MESSAGES TO BE Serialized to ByteMsg using Message Pack  ]
//--------------------------------------------------------------------------------------------------------------------------------------------


[MessagePackObject]
public record struct MsgRegisterMicroService(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgMicroServiceHealthSummaryRequest(
    [property: Key(0)] long Ts,
    [property: Key(1)] string Text
);
  

[MessagePackObject]
public record struct MsgMicroServiceHealthSummaryNotification(
    [property: Key(0)] long Ts,
    [property: Key(1)] long ServiceStartedTs,
    [property: Key(2)] string ServiceName,
    [property: Key(3)] string ServiceUrl,
    [property: Key(4)] string ServiceVersion,
    [property: Key(5)] string SharedVersion,
    [property: Key(6)] long SchemaVersion,
    [property: Key(7)] long TotalCountExposedEndpoints,
    [property: Key(8)] long TotalCount,
    [property: Key(9)] long TotalCountLogs,
    [property: Key(10)] long TotalCountLogsDebug,
    [property: Key(11)] long TotalCountLogsInformation,
    [property: Key(12)] long TotalCountLogsWarning,
    [property: Key(13)] long TotalCountLogsError,
    [property: Key(14)] long TotalCountLogsFatal,
    [property: Key(15)] long TotalCountMsgIn,
    [property: Key(16)] long TotalCountMsgOut,
    [property: Key(17)] long TotalCountInfluxRead,
    [property: Key(18)] long TotalCountInfluxWrite,
    [property: Key(19)] long TotalCountAuth,
    [property: Key(20)] long TotalCountFirestoreRead,
    [property: Key(21)] long TotalCountFirestoreWrite,
    [property: Key(22)] long CurrentCountConnectedStreamClients
);
    
[MessagePackObject]
public record struct MsgMicroServiceHealthSummarySnapshot(
    [property: Key(0)] long Ts,
    [property: Key(1)] MsgMicroServiceHealthSummaryNotification[] MicroServices
);
    

[MessagePackObject]
public record struct MsgChat(
    [property: Key(0)] long Ts,
    [property: Key(1)] string Text
);
    

[MessagePackObject]
public record struct MsgTradingConnectionStatus(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string Status,
    [property: Key(3)] bool IsConnected,
    [property: Key(4)] bool IsError,
    [property: Key(5)] string ErrorMessage


);
    

[MessagePackObject]
public record struct MsgTradingConnectionSubscriptionStatus(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string SymbolKey,
    [property: Key(3)] string SymbolRouting,    
    [property: Key(4)] string Status,
    [property: Key(5)] bool IsSubscribed,
    [property: Key(6)] bool IsError,
    [property: Key(7)] string ErrorMessage
);
    

[MessagePackObject]
public record struct MsgTradingConnectionTradeStatus(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string SymbolKey,
    [property: Key(3)] string SymbolRouting, 
    [property: Key(4)] string TradeId,    
    [property: Key(5)] string Status,
    [property: Key(6)] bool IsBuy
);
    
[MessagePackObject]
public record struct MsgStartTradingAccountRequest(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string TradingAccountProvider,
    [property: Key(3)] int BrokerId,
    [property: Key(4)] string TradingAccountConnectionString,
    [property: Key(5)] long LastConnectionTs,
    [property: Key(6)] SubMsgStartTradingAccountSubscribeItem[] Subscribe
);

[MessagePackObject]
public record struct SubMsgStartTradingAccountSubscribeItem(
    [property: Key(0)] string SymbolKey,
    [property: Key(1)] string SymbolRouting,
    [property: Key(2)] int Digits
);
    
[MessagePackObject]
public record struct MsgStopTradingAccountRequest(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId
);
    
[MessagePackObject]
public record struct MsgSubscribeSymbolRequest(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string SymbolKey,
    [property: Key(3)] string SymbolRouting,
    [property: Key(4)] int Digits
);
    
[MessagePackObject]
public record struct MsgUnSubscribeSymbolRequest(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string SymbolKey
);
    

[MessagePackObject]
public record struct MsgOpenTradeManualRequest(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string SymbolKey,
    [property: Key(3)] int Digits,
    [property: Key(4)] double LotSize,
    [property: Key(5)] bool IsBuy,
    [property: Key(6)] double Price
);
    

[MessagePackObject]
public record struct MsgCloseTradeManualRequest(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string TradeId,
    [property: Key(3)] string SymbolKey,
    [property: Key(4)] int Digits,
    [property: Key(5)] double LotSize,
    [property: Key(6)] bool IsBuy,
    [property: Key(7)] double Price
);
    

[MessagePackObject]
public record struct MsgStartTradingAccountResponse(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string TradingAccountProvider,
    [property: Key(3)] int BrokerId,
    [property: Key(4)] string Message,
    [property: Key(5)] string[] SubscribedSymbolKeys,
    [property: Key(6)] string[] FailedToSubscribeSymbolKeys
);
    

[MessagePackObject]
public record struct MsgStopTradingAccountResponse(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string TradingAccountProvider,
    [property: Key(3)] int BrokerId,
    [property: Key(4)] string Message
);
    

[MessagePackObject]
public record struct MsgSubscribeSymbolResponse(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string TradingAccountProvider,
    [property: Key(3)] int BrokerId,
    [property: Key(4)] string SymbolKey,
    [property: Key(5)] string SymbolRouting,
    [property: Key(6)] int Digits,
    [property: Key(7)] string Message
);
    

[MessagePackObject]
public record struct MsgUnSubscribeSymbolResponse(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string TradingAccountProvider,
    [property: Key(3)] int BrokerId,
    [property: Key(4)] string SymbolKey,
    [property: Key(5)] string SymbolRouting,
    [property: Key(6)] int Digits,
    [property: Key(7)] string Message
);
    

[MessagePackObject]
public record struct MsgOpenTradeManualResponse(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string TradeId,
    [property: Key(3)] string SymbolKey,
    [property: Key(4)] int Digits,
    [property: Key(5)] double LotSize,
    [property: Key(6)] bool IsBuy,
    [property: Key(7)] double Price,
    [property: Key(8)] bool IsError,
    [property: Key(9)] string ErrorMessage
);
    

[MessagePackObject]
public record struct MsgCloseTradeManualResponse(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string TradeId,
    [property: Key(3)] string SymbolKey,
    [property: Key(4)] int Digits,
    [property: Key(5)] double LotSize,
    [property: Key(6)] bool IsBuy,
    [property: Key(7)] double Price,
    [property: Key(8)] bool IsError,
    [property: Key(9)] string ErrorMessage


);
    

[MessagePackObject]
public record struct MsgMDRoutingBulk(
  [property: Key(0)] long Ts,
  [property: Key(1)] SubMsgMDRouting[] Ticks
);

[MessagePackObject]
public record struct SubMsgMDRouting(
  [property: Key(0)] long IncomingTickTs,
  [property: Key(1)] long ProcessedTickTs,
  [property: Key(2)] double ProcessingMs,
  [property: Key(3)] string SymbolKey,
  [property: Key(4)] string SymbolRouting,
  [property: Key(5)] int Digits,
  [property: Key(6)] double Ask,
  [property: Key(7)] double Bid,
  [property: Key(8)] double AveragePrice,
  [property: Key(9)] double Spread,
  [property: Key(10)] SubMsgMDRoutingItem[] TradingConnectionQuotes
);

[MessagePackObject]
public record struct SubMsgMDRoutingItem(
  [property: Key(0)] long Ts,
  [property: Key(1)] string OrganizationId,
  [property: Key(2)] string TradingAccountId,
  [property: Key(3)] string SymbolKey,
  [property: Key(4)] string SymbolRouting,
  [property: Key(5)] int Digits,
  [property: Key(6)] double Ask,
  [property: Key(7)] double Bid,
  [property: Key(8)] double AveragePrice,
  [property: Key(9)] double Spread,
  [property: Key(10)] double AskOffset,
  [property: Key(11)] double BidOffset,
  [property: Key(12)] double AskAfterOffset,
  [property: Key(13)] double BidAfterOffset,
  [property: Key(14)] double BuyGap,
  [property: Key(15)] double SellGap,
  [property: Key(16)] bool IsBuyGap,
  [property: Key(17)] bool IsSellGap
);
  
[MessagePackObject]
public record struct MsgSnapshotDCNodesSlim(
    [property: Key(0)] long Ts,
    [property: Key(1)] SubMsgSnapshotDCNodeSlimItem[] Nodes
);

[MessagePackObject]
public record struct SubMsgSnapshotDCNodeSlimItem(
    [property: Key(0)] string ConnectionId,
    [property: Key(1)] string IpAddress,
    [property: Key(2)] string OrganizationId,
    [property: Key(3)] string Secret,
    [property: Key(4)] string Version,
    [property: Key(5)] string SchemaVersion,
    [property: Key(6)] string SharedNodeVersion,
    [property: Key(7)] string NodeId
);
    
[MessagePackObject]
public record struct MsgSnapshotSingleDCNode(
    [property: Key(0)] long Ts,
    [property: Key(1)] long StartedAt,
    [property: Key(2)] string NodeId,
    [property: Key(3)] string OrganizationId,
    [property: Key(4)] string Version,
    [property: Key(5)] string SchemaVersion,
    [property: Key(6)] string SharedNodeVersion,
    [property: Key(7)] string ConnectionStatusTC,
    [property: Key(8)] string PingTC,
    [property: Key(9)] int SymbolsTC,
    [property: Key(10)] int StreamingSymbols,
    [property: Key(11)] int ConnectedClients,
    [property: Key(12)] int Tps,
    [property: Key(13)] int TicksLast24h,
    [property: Key(14)] long TotalProcessedTicks,
    [property: Key(15)] int TickToDistributeMs
);
    
[MessagePackObject]
public record struct MsgSnapshotGroupDCNodes(
    [property: Key(0)] long Ts,
    [property: Key(1)] List<MsgSnapshotSingleDCNode> Nodes
);
  
[MessagePackObject]
public record struct MsgSnapshotNodesSlim(
    [property: Key(0)] long Ts,
    [property: Key(1)] SubMsgSnapshotNodeSlimItem[] Nodes
);

[MessagePackObject]
public record struct SubMsgSnapshotNodeSlimItem(
    [property: Key(0)] string ConnectionId,
    [property: Key(1)] string IpAddress,
    [property: Key(2)] string OrganizationId,
    [property: Key(3)] string Secret,
    [property: Key(4)] string Version,
    [property: Key(5)] string SchemaVersion,
    [property: Key(6)] string SharedNodeVersion,
    [property: Key(7)] string NodeId
);
    
[MessagePackObject]
public record struct MsgSnapshotFullSingleAgentNode(
    [property: Key(0)] long Ts,
    [property: Key(1)] string OrganizationId,
    [property: Key(2)] string NodeId,
    [property: Key(3)] string OperatingSystem,
    [property: Key(4)] string CountryCode,
    [property: Key(5)] List<SubMsgSnapshotFullSingleAgentNodeTC> ConnectedAccounts
);

[MessagePackObject]
public record struct SubMsgSnapshotFullSingleAgentNodeTC(
    [property: Key(0)] string OrganizationId,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string ConnectionStatus,
    [property: Key(3)] double Balance,
    [property: Key(4)] double Equity,
    [property: Key(5)] double PingMs,
    [property: Key(6)] int StreamingSymbols,
    [property: Key(7)] int TpsAccount,
    [property: Key(8)] int TpmAccount,
    [property: Key(9)] int OpenedTradesCount,
    [property: Key(10)] int ErrorCount,
    [property: Key(11)] long ConnectedAtTs
);
    
[MessagePackObject]
public record struct MsgSnapshotSingleAgentInformation(
    [property: Key(0)] long Ts,
    [property: Key(1)] string OrganizationId,
    [property: Key(2)] string NodeId,
    [property: Key(3)] string OperatingSystem,
    [property: Key(4)] string CountryCode,
    [property: Key(5)] int TpsTotal,
    [property: Key(6)] int TickToTradeMax,
    [property: Key(7)] int TickToTradeMin,
    [property: Key(8)] int TickToTradeAverage,
    [property: Key(9)] int ConnectedTradingAccountsCount,
    [property: Key(10)] int StreamingMarketDataSymbolsCount
);


    
[MessagePackObject]
public record struct MsgSnapshotSingleAgentStatus(
    [property: Key(0)] long Ts,
    [property: Key(1)] string OrganizationId,
    [property: Key(2)] string NodeId,
    [property: Key(3)] string OperatingSystem,
    [property: Key(4)] string HardwareCPU,
    [property: Key(5)] string HardwareNC,
    [property: Key(6)] string HardwareGPU,
    [property: Key(7)] string Country,
    [property: Key(8)] string CountryCode,
    [property: Key(9)] string City,
    [property: Key(10)] string Region,
    [property: Key(11)] string RegionName,
    [property: Key(12)] string Zip,
    [property: Key(13)] double Lat,
    [property: Key(14)] double Lon,
    [property: Key(15)] string Timezone,
    [property: Key(16)] string Isp,
    [property: Key(17)] string OrgServer,
    [property: Key(18)] string QueryDNS,
    [property: Key(19)] string OriginIp
);
  
[MessagePackObject]
public record struct MsgSnapshotSingleAgentTradingConnections(
    [property: Key(0)] long Ts,
    [property: Key(1)] string OrganizationId,
    [property: Key(2)] string NodeId,
    [property: Key(3)] string OperatingSystem,
    [property: Key(4)] string CountryCode,
    [property: Key(5)] List<SubMsgSnapshotSingleAgentTradingConnectionsItem> ConnectedAccounts
);

[MessagePackObject]
public record struct SubMsgSnapshotSingleAgentTradingConnectionsItem(
    [property: Key(0)] string OrganizationId,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string ConnectionStatus,
    [property: Key(3)] double Balance,
    [property: Key(4)] double Equity,
    [property: Key(5)] double PingMs,
    [property: Key(6)] int StreamingSymbols,
    [property: Key(7)] int TpsAccount,
    [property: Key(8)] int TpmAccount,
    [property: Key(9)] int OpenedTradesCount,
    [property: Key(10)] int ErrorCount,
    [property: Key(11)] long ConnectedAtTs
);
    
[MessagePackObject]
public record struct MsgSnapshotSingleAgentMarketData(
    [property: Key(0)] long Ts,
    [property: Key(1)] int TotalTps,
    [property: Key(2)] int SymbolsCount,
    [property: Key(3)] string NodeId,
    [property: Key(4)] List<SubMsgSnapshotSingleAgentMarketDataQuote> Quotes
);

[MessagePackObject]
public record struct SubMsgSnapshotSingleAgentMarketDataQuote(
    [property: Key(0)] long Ts,
    [property: Key(1)] long TickNumber,
    [property: Key(2)] string OrganizationId,
    [property: Key(3)] string TradingAccountId,
    [property: Key(4)] string SymbolKey,
    [property: Key(5)] string SymbolRouting,
    [property: Key(6)] int Digits,
    [property: Key(7)] double Ask,
    [property: Key(8)] double Bid,
    [property: Key(9)] double AveragePrice,
    [property: Key(10)] double Spread,
    [property: Key(11)] int Tps,
    [property: Key(12)] int Tpm,
    [property: Key(13)] double Delta1M,
    [property: Key(14)] double Delta5M
);
    
[MessagePackObject]
public record struct MsgSnapshotSingleAgentLiveTrades(
    [property: Key(0)] long Ts,
    [property: Key(1)] double TotalPnL,
    [property: Key(2)] int TradesCount,
    [property: Key(3)] string NodeId,
    [property: Key(4)] List<SubMsgSnapshotSingleAgentLiveTradesItem> Trades
);

[MessagePackObject]
public record struct SubMsgSnapshotSingleAgentLiveTradesItem(
    [property: Key(0)] long TradeOpenedTs,    
    [property: Key(1)] string OrganizationId,
    [property: Key(2)] string TradingAccountId,
    [property: Key(3)] string TradeId,
    [property: Key(4)] string TradeTicket,
    [property: Key(5)] string Direction,
    [property: Key(6)] string SymbolKey,
    [property: Key(7)] int Digits,
    [property: Key(8)] double OpenPrice,
    [property: Key(9)] double PnL,
    [property: Key(10)] double TP,
    [property: Key(11)] double SL
);
    
[MessagePackObject]
public record struct MsgSnapshotSingleAgentMetrics(
    [property: Key(0)] long Ts,
    [property: Key(1)] string OrganizationId,
    [property: Key(2)] string NodeId,
    [property: Key(3)] string OperatingSystem,
    [property: Key(4)] string HardwareCPU,
    [property: Key(5)] string HardwareNC,
    [property: Key(6)] string HardwareGPU,
    [property: Key(7)] long ProcessedRequests,
    [property: Key(8)] long ProcessedTicks,
    [property: Key(9)] long ProcessedSignals,
    [property: Key(10)] long ProcessedTrades,
    [property: Key(11)] long CountTradingConnections,
    [property: Key(12)] long CountStreamingSymbols,
    [property: Key(13)] long TickToTradeAbove1Ms,
    [property: Key(14)] long TickToTradeBelow1Ms,
    [property: Key(15)] double TickToTradeAverageMs
);
  
  [MessagePackObject]
  public record struct MsgTCLog(
      [property: Key(0)] long Ts,
      [property: Key(1)] string TradingAccountId,
      [property: Key(2)] int Severity,
      [property: Key(3)] string Message
  );
      

//--------------------------------------------------------------------------------------------------------------------------------------------
//------[ MESSAGE EVENT GATEWAY WITH HANDLERS ON ALREADY DECODED EVENTS AND SEND FUNCTIONS TO PUBLISH EVENTS ]
//--------------------------------------------------------------------------------------------------------------------------------------------

public interface IMsgPublisher
{
    public void Publish(RabbitMsgWrapper msg);
    public long MsgTimestamp {get;}
    public long MsgCounter {get;}
}

public interface IEventGatewayConfiguration
{
    public string ServiceName {get;}
    public string ServiceVersion {get;}
    public string ServiceSharedVersion {get;}
}

public class EventGateway
{
    private readonly IMsgPublisher _publisher;
    private readonly IEventGatewayConfiguration _eventGatewayConfiguration;

    public EventGateway(IMsgPublisher publisher, IEventGatewayConfiguration eventGatewayConfiguration)
    {
        _eventGatewayConfiguration = eventGatewayConfiguration;
        _publisher = publisher;
    }

    public EventGateway(IEventGatewayConfiguration eventGatewayConfiguration)
    {
        _eventGatewayConfiguration = eventGatewayConfiguration;
        _publisher = null;
    }

    public event Action<RabbitMsgWrapper> OnNewMsg;

    public void Send(RabbitMsgWrapper msg)
    {
        _publisher.Publish(msg);
    }

    
    public event Action<RabbitMsgWrapper, MsgRegisterMicroService> EventMsgRegisterMicroService;
    public event Action<RabbitMsgWrapper, MsgMicroServiceHealthSummaryRequest> EventMsgMicroServiceHealthSummaryRequest;
    public event Action<RabbitMsgWrapper, MsgMicroServiceHealthSummaryNotification> EventMsgMicroServiceHealthSummaryNotification;
    public event Action<RabbitMsgWrapper, MsgMicroServiceHealthSummarySnapshot> EventMsgMicroServiceHealthSummarySnapshot;
    public event Action<RabbitMsgWrapper, MsgChat> EventMsgChat;
    public event Action<RabbitMsgWrapper, MsgTradingConnectionStatus> EventMsgTradingConnectionStatus;
    public event Action<RabbitMsgWrapper, MsgTradingConnectionSubscriptionStatus> EventMsgTradingConnectionSubscriptionStatus;
    public event Action<RabbitMsgWrapper, MsgTradingConnectionTradeStatus> EventMsgTradingConnectionTradeStatus;
    public event Action<RabbitMsgWrapper, MsgStartTradingAccountRequest> EventMsgStartTradingAccountRequest;
    public event Action<RabbitMsgWrapper, MsgStopTradingAccountRequest> EventMsgStopTradingAccountRequest;
    public event Action<RabbitMsgWrapper, MsgSubscribeSymbolRequest> EventMsgSubscribeSymbolRequest;
    public event Action<RabbitMsgWrapper, MsgUnSubscribeSymbolRequest> EventMsgUnSubscribeSymbolRequest;
    public event Action<RabbitMsgWrapper, MsgOpenTradeManualRequest> EventMsgOpenTradeManualRequest;
    public event Action<RabbitMsgWrapper, MsgCloseTradeManualRequest> EventMsgCloseTradeManualRequest;
    public event Action<RabbitMsgWrapper, MsgStartTradingAccountResponse> EventMsgStartTradingAccountResponse;
    public event Action<RabbitMsgWrapper, MsgStopTradingAccountResponse> EventMsgStopTradingAccountResponse;
    public event Action<RabbitMsgWrapper, MsgSubscribeSymbolResponse> EventMsgSubscribeSymbolResponse;
    public event Action<RabbitMsgWrapper, MsgUnSubscribeSymbolResponse> EventMsgUnSubscribeSymbolResponse;
    public event Action<RabbitMsgWrapper, MsgOpenTradeManualResponse> EventMsgOpenTradeManualResponse;
    public event Action<RabbitMsgWrapper, MsgCloseTradeManualResponse> EventMsgCloseTradeManualResponse;
    public event Action<RabbitMsgWrapper, MsgMDRoutingBulk> EventMsgMDRoutingBulk;
    public event Action<RabbitMsgWrapper, MsgSnapshotDCNodesSlim> EventMsgSnapshotDCNodesSlim;
    public event Action<RabbitMsgWrapper, MsgSnapshotSingleDCNode> EventMsgSnapshotSingleDCNode;
    public event Action<RabbitMsgWrapper, MsgSnapshotGroupDCNodes> EventMsgSnapshotGroupDCNodes;
    public event Action<RabbitMsgWrapper, MsgSnapshotNodesSlim> EventMsgSnapshotNodesSlim;
    public event Action<RabbitMsgWrapper, MsgSnapshotFullSingleAgentNode> EventMsgSnapshotFullSingleAgentNode;
    public event Action<RabbitMsgWrapper, MsgSnapshotSingleAgentInformation> EventMsgSnapshotSingleAgentInformation;
    public event Action<RabbitMsgWrapper, MsgSnapshotSingleAgentStatus> EventMsgSnapshotSingleAgentStatus;
    public event Action<RabbitMsgWrapper, MsgSnapshotSingleAgentTradingConnections> EventMsgSnapshotSingleAgentTradingConnections;
    public event Action<RabbitMsgWrapper, MsgSnapshotSingleAgentMarketData> EventMsgSnapshotSingleAgentMarketData;
    public event Action<RabbitMsgWrapper, MsgSnapshotSingleAgentLiveTrades> EventMsgSnapshotSingleAgentLiveTrades;
    public event Action<RabbitMsgWrapper, MsgSnapshotSingleAgentMetrics> EventMsgSnapshotSingleAgentMetrics;
    public event Action<RabbitMsgWrapper, MsgTCLog> EventMsgTCLog;
  
    public string Send(MsgRegisterMicroService @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.micro.services.mesh",
            MessageType = "MsgRegisterMicroService",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgRegisterMicroService " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgMicroServiceHealthSummaryRequest @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.micro.services.mesh",
            MessageType = "MsgMicroServiceHealthSummaryRequest",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgMicroServiceHealthSummaryRequest " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgMicroServiceHealthSummaryNotification @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.micro.services.mesh",
            MessageType = "MsgMicroServiceHealthSummaryNotification",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgMicroServiceHealthSummaryNotification " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgMicroServiceHealthSummarySnapshot @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.micro.services.mesh",
            MessageType = "MsgMicroServiceHealthSummarySnapshot",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgMicroServiceHealthSummarySnapshot " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgChat @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.chat",
            MessageType = "MsgChat",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgChat " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgTradingConnectionStatus @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.status",
            MessageType = "MsgTradingConnectionStatus",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgTradingConnectionStatus " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgTradingConnectionSubscriptionStatus @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.status",
            MessageType = "MsgTradingConnectionSubscriptionStatus",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgTradingConnectionSubscriptionStatus " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgTradingConnectionTradeStatus @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.status",
            MessageType = "MsgTradingConnectionTradeStatus",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgTradingConnectionTradeStatus " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgStartTradingAccountRequest @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.request",
            MessageType = "MsgStartTradingAccountRequest",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgStartTradingAccountRequest " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgStopTradingAccountRequest @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.request",
            MessageType = "MsgStopTradingAccountRequest",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgStopTradingAccountRequest " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSubscribeSymbolRequest @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.request",
            MessageType = "MsgSubscribeSymbolRequest",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSubscribeSymbolRequest " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgUnSubscribeSymbolRequest @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.request",
            MessageType = "MsgUnSubscribeSymbolRequest",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgUnSubscribeSymbolRequest " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgOpenTradeManualRequest @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.request",
            MessageType = "MsgOpenTradeManualRequest",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgOpenTradeManualRequest " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgCloseTradeManualRequest @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.request",
            MessageType = "MsgCloseTradeManualRequest",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgCloseTradeManualRequest " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgStartTradingAccountResponse @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.response",
            MessageType = "MsgStartTradingAccountResponse",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgStartTradingAccountResponse " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgStopTradingAccountResponse @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.response",
            MessageType = "MsgStopTradingAccountResponse",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgStopTradingAccountResponse " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSubscribeSymbolResponse @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.response",
            MessageType = "MsgSubscribeSymbolResponse",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSubscribeSymbolResponse " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgUnSubscribeSymbolResponse @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.response",
            MessageType = "MsgUnSubscribeSymbolResponse",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgUnSubscribeSymbolResponse " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgOpenTradeManualResponse @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.response",
            MessageType = "MsgOpenTradeManualResponse",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgOpenTradeManualResponse " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgCloseTradeManualResponse @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.response",
            MessageType = "MsgCloseTradeManualResponse",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgCloseTradeManualResponse " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgMDRoutingBulk @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.agent.md",
            MessageType = "MsgMDRoutingBulk",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgMDRoutingBulk " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSnapshotDCNodesSlim @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.snapshot.notification",
            MessageType = "MsgSnapshotDCNodesSlim",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSnapshotDCNodesSlim " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSnapshotSingleDCNode @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.snapshot.notification",
            MessageType = "MsgSnapshotSingleDCNode",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSnapshotSingleDCNode " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSnapshotGroupDCNodes @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.snapshot.notification",
            MessageType = "MsgSnapshotGroupDCNodes",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSnapshotGroupDCNodes " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSnapshotNodesSlim @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.snapshot.notification",
            MessageType = "MsgSnapshotNodesSlim",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSnapshotNodesSlim " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSnapshotFullSingleAgentNode @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.snapshot.notification",
            MessageType = "MsgSnapshotFullSingleAgentNode",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSnapshotFullSingleAgentNode " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSnapshotSingleAgentInformation @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.snapshot.notification",
            MessageType = "MsgSnapshotSingleAgentInformation",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSnapshotSingleAgentInformation " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSnapshotSingleAgentStatus @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.snapshot.notification",
            MessageType = "MsgSnapshotSingleAgentStatus",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSnapshotSingleAgentStatus " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSnapshotSingleAgentTradingConnections @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.snapshot.notification",
            MessageType = "MsgSnapshotSingleAgentTradingConnections",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSnapshotSingleAgentTradingConnections " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSnapshotSingleAgentMarketData @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.snapshot.notification",
            MessageType = "MsgSnapshotSingleAgentMarketData",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSnapshotSingleAgentMarketData " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSnapshotSingleAgentLiveTrades @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.snapshot.notification",
            MessageType = "MsgSnapshotSingleAgentLiveTrades",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSnapshotSingleAgentLiveTrades " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSnapshotSingleAgentMetrics @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.snapshot.notification",
            MessageType = "MsgSnapshotSingleAgentMetrics",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgSnapshotSingleAgentMetrics " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgTCLog @event,
        long? ts = null,
        string severity = null,
        string actionId = null,
        string organizationId = null,
        string userId = null,
        string nodeId = null,
        string userEmail = null
    )
    {
        var msg = new RabbitMsgWrapper()
        {
            Ts = ts ?? DateTime.UtcNow.Ticks,
            Severity = severity ?? "Information",
            ActionId = actionId ?? Guid.NewGuid().ToString(),
            Exchange = "exchange.tc.logs",
            MessageType = "MsgTCLog",
            ByteMsg = MessagePackSerializer.Serialize(@event),
            OrganizationId = organizationId ?? "System",
            UserId = userId ?? "System",
            NodeId = nodeId ?? "System",
            UserEmail = userEmail ?? "System",

            AppName = _eventGatewayConfiguration.ServiceName,
            AppVersion = _eventGatewayConfiguration.ServiceVersion,
            SharedVersion = _eventGatewayConfiguration.ServiceSharedVersion,
            SchemaVersion = MsgSchema.Version.ToString()
        };

        _publisher.Publish(msg);
        //Console.WriteLine("Sent MsgTCLog " + msg.ToString());
        return msg.ActionId;
    }
    
    public void Receive(BasicDeliverEventArgs args)
    {
        Receive(MsgSchema.FromRabbit(args));
    }

    public void Receive(RabbitMsgWrapper msg)
    {
        OnNewMsg?.Invoke(msg);

        
        if (msg.MessageType == "MsgRegisterMicroService")
        {
            EventMsgRegisterMicroService?.Invoke(msg, MessagePackSerializer.Deserialize<MsgRegisterMicroService>(msg.ByteMsg));
            //Console.WriteLine("Received MsgRegisterMicroService " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgMicroServiceHealthSummaryRequest")
        {
            EventMsgMicroServiceHealthSummaryRequest?.Invoke(msg, MessagePackSerializer.Deserialize<MsgMicroServiceHealthSummaryRequest>(msg.ByteMsg));
            //Console.WriteLine("Received MsgMicroServiceHealthSummaryRequest " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgMicroServiceHealthSummaryNotification")
        {
            EventMsgMicroServiceHealthSummaryNotification?.Invoke(msg, MessagePackSerializer.Deserialize<MsgMicroServiceHealthSummaryNotification>(msg.ByteMsg));
            //Console.WriteLine("Received MsgMicroServiceHealthSummaryNotification " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgMicroServiceHealthSummarySnapshot")
        {
            EventMsgMicroServiceHealthSummarySnapshot?.Invoke(msg, MessagePackSerializer.Deserialize<MsgMicroServiceHealthSummarySnapshot>(msg.ByteMsg));
            //Console.WriteLine("Received MsgMicroServiceHealthSummarySnapshot " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgChat")
        {
            EventMsgChat?.Invoke(msg, MessagePackSerializer.Deserialize<MsgChat>(msg.ByteMsg));
            //Console.WriteLine("Received MsgChat " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgTradingConnectionStatus")
        {
            EventMsgTradingConnectionStatus?.Invoke(msg, MessagePackSerializer.Deserialize<MsgTradingConnectionStatus>(msg.ByteMsg));
            //Console.WriteLine("Received MsgTradingConnectionStatus " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgTradingConnectionSubscriptionStatus")
        {
            EventMsgTradingConnectionSubscriptionStatus?.Invoke(msg, MessagePackSerializer.Deserialize<MsgTradingConnectionSubscriptionStatus>(msg.ByteMsg));
            //Console.WriteLine("Received MsgTradingConnectionSubscriptionStatus " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgTradingConnectionTradeStatus")
        {
            EventMsgTradingConnectionTradeStatus?.Invoke(msg, MessagePackSerializer.Deserialize<MsgTradingConnectionTradeStatus>(msg.ByteMsg));
            //Console.WriteLine("Received MsgTradingConnectionTradeStatus " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgStartTradingAccountRequest")
        {
            EventMsgStartTradingAccountRequest?.Invoke(msg, MessagePackSerializer.Deserialize<MsgStartTradingAccountRequest>(msg.ByteMsg));
            //Console.WriteLine("Received MsgStartTradingAccountRequest " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgStopTradingAccountRequest")
        {
            EventMsgStopTradingAccountRequest?.Invoke(msg, MessagePackSerializer.Deserialize<MsgStopTradingAccountRequest>(msg.ByteMsg));
            //Console.WriteLine("Received MsgStopTradingAccountRequest " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSubscribeSymbolRequest")
        {
            EventMsgSubscribeSymbolRequest?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSubscribeSymbolRequest>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSubscribeSymbolRequest " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgUnSubscribeSymbolRequest")
        {
            EventMsgUnSubscribeSymbolRequest?.Invoke(msg, MessagePackSerializer.Deserialize<MsgUnSubscribeSymbolRequest>(msg.ByteMsg));
            //Console.WriteLine("Received MsgUnSubscribeSymbolRequest " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgOpenTradeManualRequest")
        {
            EventMsgOpenTradeManualRequest?.Invoke(msg, MessagePackSerializer.Deserialize<MsgOpenTradeManualRequest>(msg.ByteMsg));
            //Console.WriteLine("Received MsgOpenTradeManualRequest " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgCloseTradeManualRequest")
        {
            EventMsgCloseTradeManualRequest?.Invoke(msg, MessagePackSerializer.Deserialize<MsgCloseTradeManualRequest>(msg.ByteMsg));
            //Console.WriteLine("Received MsgCloseTradeManualRequest " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgStartTradingAccountResponse")
        {
            EventMsgStartTradingAccountResponse?.Invoke(msg, MessagePackSerializer.Deserialize<MsgStartTradingAccountResponse>(msg.ByteMsg));
            //Console.WriteLine("Received MsgStartTradingAccountResponse " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgStopTradingAccountResponse")
        {
            EventMsgStopTradingAccountResponse?.Invoke(msg, MessagePackSerializer.Deserialize<MsgStopTradingAccountResponse>(msg.ByteMsg));
            //Console.WriteLine("Received MsgStopTradingAccountResponse " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSubscribeSymbolResponse")
        {
            EventMsgSubscribeSymbolResponse?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSubscribeSymbolResponse>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSubscribeSymbolResponse " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgUnSubscribeSymbolResponse")
        {
            EventMsgUnSubscribeSymbolResponse?.Invoke(msg, MessagePackSerializer.Deserialize<MsgUnSubscribeSymbolResponse>(msg.ByteMsg));
            //Console.WriteLine("Received MsgUnSubscribeSymbolResponse " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgOpenTradeManualResponse")
        {
            EventMsgOpenTradeManualResponse?.Invoke(msg, MessagePackSerializer.Deserialize<MsgOpenTradeManualResponse>(msg.ByteMsg));
            //Console.WriteLine("Received MsgOpenTradeManualResponse " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgCloseTradeManualResponse")
        {
            EventMsgCloseTradeManualResponse?.Invoke(msg, MessagePackSerializer.Deserialize<MsgCloseTradeManualResponse>(msg.ByteMsg));
            //Console.WriteLine("Received MsgCloseTradeManualResponse " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgMDRoutingBulk")
        {
            EventMsgMDRoutingBulk?.Invoke(msg, MessagePackSerializer.Deserialize<MsgMDRoutingBulk>(msg.ByteMsg));
            //Console.WriteLine("Received MsgMDRoutingBulk " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotDCNodesSlim")
        {
            EventMsgSnapshotDCNodesSlim?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotDCNodesSlim>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSnapshotDCNodesSlim " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotSingleDCNode")
        {
            EventMsgSnapshotSingleDCNode?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotSingleDCNode>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSnapshotSingleDCNode " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotGroupDCNodes")
        {
            EventMsgSnapshotGroupDCNodes?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotGroupDCNodes>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSnapshotGroupDCNodes " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotNodesSlim")
        {
            EventMsgSnapshotNodesSlim?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotNodesSlim>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSnapshotNodesSlim " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotFullSingleAgentNode")
        {
            EventMsgSnapshotFullSingleAgentNode?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotFullSingleAgentNode>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSnapshotFullSingleAgentNode " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotSingleAgentInformation")
        {
            EventMsgSnapshotSingleAgentInformation?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotSingleAgentInformation>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSnapshotSingleAgentInformation " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotSingleAgentStatus")
        {
            EventMsgSnapshotSingleAgentStatus?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotSingleAgentStatus>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSnapshotSingleAgentStatus " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotSingleAgentTradingConnections")
        {
            EventMsgSnapshotSingleAgentTradingConnections?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotSingleAgentTradingConnections>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSnapshotSingleAgentTradingConnections " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotSingleAgentMarketData")
        {
            EventMsgSnapshotSingleAgentMarketData?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotSingleAgentMarketData>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSnapshotSingleAgentMarketData " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotSingleAgentLiveTrades")
        {
            EventMsgSnapshotSingleAgentLiveTrades?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotSingleAgentLiveTrades>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSnapshotSingleAgentLiveTrades " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotSingleAgentMetrics")
        {
            EventMsgSnapshotSingleAgentMetrics?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotSingleAgentMetrics>(msg.ByteMsg));
            //Console.WriteLine("Received MsgSnapshotSingleAgentMetrics " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgTCLog")
        {
            EventMsgTCLog?.Invoke(msg, MessagePackSerializer.Deserialize<MsgTCLog>(msg.ByteMsg));
            //Console.WriteLine("Received MsgTCLog " + msg.ToString());
            return;
        }
        }

}


    