
// MsgSchema.cs
global using MessagePack;
global using RabbitMQ.Client.Events;
global using HFTbridge.Msg;
using System.Text;

namespace HFTbridge.Msg;

public static class MsgSchema
{
    public static int Version = 19;

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
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgSymbolStatus(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    
[MessagePackObject]
public record struct MsgStartTradingAccount(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string TradingAccountProvider,
    [property: Key(3)] string TradingAccountConnectionString,
    [property: Key(4)] long LastConnectionTs,
    [property: Key(5)] SubMsgStartTradingAccountSubscribeItem[] Subscribe
);

[MessagePackObject]
public record struct SubMsgStartTradingAccountSubscribeItem(
    [property: Key(0)] string SymbolKey,
    [property: Key(1)] string SymbolRouting,
    [property: Key(2)] int Digits
);
    
[MessagePackObject]
public record struct MsgStopTradingAccount(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId
);
    
[MessagePackObject]
public record struct MsgSubscribeSymbol(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string SymbolKey,
    [property: Key(3)] string SymbolRouting,
    [property: Key(4)] int Digits
);
    
[MessagePackObject]
public record struct MsgUnSubscribeSymbol(
    [property: Key(0)] long Ts,
    [property: Key(1)] string TradingAccountId,
    [property: Key(2)] string SymbolKey
);
    

[MessagePackObject]
public record struct MsgOpenTradeManual(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgStartTradingAccountResponse(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgStopTradingAccountResponse(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgSubscribeSymbolResponse(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgUnSubscribeSymbolResponse(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgOpenTradeManualResponse(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgMDTick(
    [property: Key(0)] long Ts,
    [property: Key(1)] long TickNr,
    [property: Key(2)] string TradingAccountId,
    [property: Key(3)] string SymboKey,
    [property: Key(4)] string SymbolRouting,
    [property: Key(5)] double Ask,
    [property: Key(6)] double Bid
);
    

[MessagePackObject]
public record struct MsgMDSnapshot(
    [property: Key(0)] long Ts,
    [property: Key(1)] int Tps,
    [property: Key(2)] int Tpm,
    [property: Key(3)] SubMsgMDSnapshotItem[] Items
);

[MessagePackObject]
public record struct SubMsgMDSnapshotItem(
    [property: Key(0)] long Ts,
    [property: Key(1)] long TickNr,
    [property: Key(2)] int Tps,
    [property: Key(3)] int Tpm,
    [property: Key(4)] string TradingAccountId,
    [property: Key(5)] string SymboKey,
    [property: Key(6)] string SymbolRouting,
    [property: Key(7)] double Ask,
    [property: Key(8)] double Bid,
    [property: Key(9)] double Delta5Sec,
    [property: Key(10)] double Delta30Sec,
    [property: Key(11)] double Delta60Sec
    
);
    

[MessagePackObject]
public record struct MsgMDRouting(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgTDTradeOpened(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgTDTradeClosed(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgTDSnapshot(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgSyncTradingAccount(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgStartingDC(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgStartedDC(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgRestartingDC(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgFailedToStartDC(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgDCClientConnected(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgDCClientDisconnected(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
);
    

[MessagePackObject]
public record struct MsgDCMDSnapshot(
    [property: Key(0)] long Ts,
    [property: Key(1)] int TPS,
    [property: Key(2)] int TPM,
    [property: Key(3)] int Symbols,
    [property: Key(4)] int Providers
);
    

[MessagePackObject]
public record struct MsgDCMDTick(
    [property: Key(0)] long Ts,
    [property: Key(1)] string SymbolKey,
    [property: Key(2)] string SymbolRouting
);
    

[MessagePackObject]
public record struct MsgDCRequestRestartFeed(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId
);
    

[MessagePackObject]
public record struct MsgDCRequestStartFeed(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId
);
    

[MessagePackObject]
public record struct MsgDCRequestStopFeed(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId
);
    

[MessagePackObject]
public record struct MsgDCRequestSubscribeAllSymbol(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId
);
    

[MessagePackObject]
public record struct MsgDCRequestSubscribeSymbol(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId,
    [property: Key(2)] string SymbolKey
);
    

[MessagePackObject]
public record struct MsgDCRequestUnSubscribeAllSymbol(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId
);
    

[MessagePackObject]
public record struct MsgDCRequestUnSubscribeSymbol(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId,
    [property: Key(2)] string SymbolKey
);
    

[MessagePackObject]
public record struct MsgDCResponseRestartFeed(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId,
    [property: Key(2)] bool IsSuccess
);
    

[MessagePackObject]
public record struct MsgDCResponseStartFeed(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId,
    [property: Key(2)] bool IsSuccess
);
    

[MessagePackObject]
public record struct MsgDCResponseStopFeed(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId,
    [property: Key(2)] bool IsSuccess
);
    

[MessagePackObject]
public record struct MsgDCResponseSubscribeAllSymbol(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId,
    [property: Key(2)] bool IsSuccess
);
    

[MessagePackObject]
public record struct MsgDCResponseSubscribeSymbol(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId,
    [property: Key(2)] string SymbolKey,
    [property: Key(3)] bool IsSuccess
);
    

[MessagePackObject]
public record struct MsgDCResponseUnSubscribeAllSymbol(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId,
    [property: Key(2)] bool IsSuccess
);
    

[MessagePackObject]
public record struct MsgDCResponseUnSubscribeSymbol(
    [property: Key(0)] long Ts,
    [property: Key(1)] string DcId,
    [property: Key(2)] string SymbolKey,
    [property: Key(3)] bool IsSuccess
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
    [property: Key(1)] MsgSnapshotSingleDCNode[] Nodes
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

    
    public event Action<RabbitMsgWrapper, MsgRegisterMicroService> EventMsgRegisterMicroService;
    public event Action<RabbitMsgWrapper, MsgMicroServiceHealthSummaryRequest> EventMsgMicroServiceHealthSummaryRequest;
    public event Action<RabbitMsgWrapper, MsgMicroServiceHealthSummaryNotification> EventMsgMicroServiceHealthSummaryNotification;
    public event Action<RabbitMsgWrapper, MsgMicroServiceHealthSummarySnapshot> EventMsgMicroServiceHealthSummarySnapshot;
    public event Action<RabbitMsgWrapper, MsgChat> EventMsgChat;
    public event Action<RabbitMsgWrapper, MsgTradingConnectionStatus> EventMsgTradingConnectionStatus;
    public event Action<RabbitMsgWrapper, MsgSymbolStatus> EventMsgSymbolStatus;
    public event Action<RabbitMsgWrapper, MsgStartTradingAccount> EventMsgStartTradingAccount;
    public event Action<RabbitMsgWrapper, MsgStopTradingAccount> EventMsgStopTradingAccount;
    public event Action<RabbitMsgWrapper, MsgSubscribeSymbol> EventMsgSubscribeSymbol;
    public event Action<RabbitMsgWrapper, MsgUnSubscribeSymbol> EventMsgUnSubscribeSymbol;
    public event Action<RabbitMsgWrapper, MsgOpenTradeManual> EventMsgOpenTradeManual;
    public event Action<RabbitMsgWrapper, MsgStartTradingAccountResponse> EventMsgStartTradingAccountResponse;
    public event Action<RabbitMsgWrapper, MsgStopTradingAccountResponse> EventMsgStopTradingAccountResponse;
    public event Action<RabbitMsgWrapper, MsgSubscribeSymbolResponse> EventMsgSubscribeSymbolResponse;
    public event Action<RabbitMsgWrapper, MsgUnSubscribeSymbolResponse> EventMsgUnSubscribeSymbolResponse;
    public event Action<RabbitMsgWrapper, MsgOpenTradeManualResponse> EventMsgOpenTradeManualResponse;
    public event Action<RabbitMsgWrapper, MsgMDTick> EventMsgMDTick;
    public event Action<RabbitMsgWrapper, MsgMDSnapshot> EventMsgMDSnapshot;
    public event Action<RabbitMsgWrapper, MsgMDRouting> EventMsgMDRouting;
    public event Action<RabbitMsgWrapper, MsgTDTradeOpened> EventMsgTDTradeOpened;
    public event Action<RabbitMsgWrapper, MsgTDTradeClosed> EventMsgTDTradeClosed;
    public event Action<RabbitMsgWrapper, MsgTDSnapshot> EventMsgTDSnapshot;
    public event Action<RabbitMsgWrapper, MsgSyncTradingAccount> EventMsgSyncTradingAccount;
    public event Action<RabbitMsgWrapper, MsgStartingDC> EventMsgStartingDC;
    public event Action<RabbitMsgWrapper, MsgStartedDC> EventMsgStartedDC;
    public event Action<RabbitMsgWrapper, MsgRestartingDC> EventMsgRestartingDC;
    public event Action<RabbitMsgWrapper, MsgFailedToStartDC> EventMsgFailedToStartDC;
    public event Action<RabbitMsgWrapper, MsgDCClientConnected> EventMsgDCClientConnected;
    public event Action<RabbitMsgWrapper, MsgDCClientDisconnected> EventMsgDCClientDisconnected;
    public event Action<RabbitMsgWrapper, MsgDCMDSnapshot> EventMsgDCMDSnapshot;
    public event Action<RabbitMsgWrapper, MsgDCMDTick> EventMsgDCMDTick;
    public event Action<RabbitMsgWrapper, MsgDCRequestRestartFeed> EventMsgDCRequestRestartFeed;
    public event Action<RabbitMsgWrapper, MsgDCRequestStartFeed> EventMsgDCRequestStartFeed;
    public event Action<RabbitMsgWrapper, MsgDCRequestStopFeed> EventMsgDCRequestStopFeed;
    public event Action<RabbitMsgWrapper, MsgDCRequestSubscribeAllSymbol> EventMsgDCRequestSubscribeAllSymbol;
    public event Action<RabbitMsgWrapper, MsgDCRequestSubscribeSymbol> EventMsgDCRequestSubscribeSymbol;
    public event Action<RabbitMsgWrapper, MsgDCRequestUnSubscribeAllSymbol> EventMsgDCRequestUnSubscribeAllSymbol;
    public event Action<RabbitMsgWrapper, MsgDCRequestUnSubscribeSymbol> EventMsgDCRequestUnSubscribeSymbol;
    public event Action<RabbitMsgWrapper, MsgDCResponseRestartFeed> EventMsgDCResponseRestartFeed;
    public event Action<RabbitMsgWrapper, MsgDCResponseStartFeed> EventMsgDCResponseStartFeed;
    public event Action<RabbitMsgWrapper, MsgDCResponseStopFeed> EventMsgDCResponseStopFeed;
    public event Action<RabbitMsgWrapper, MsgDCResponseSubscribeAllSymbol> EventMsgDCResponseSubscribeAllSymbol;
    public event Action<RabbitMsgWrapper, MsgDCResponseSubscribeSymbol> EventMsgDCResponseSubscribeSymbol;
    public event Action<RabbitMsgWrapper, MsgDCResponseUnSubscribeAllSymbol> EventMsgDCResponseUnSubscribeAllSymbol;
    public event Action<RabbitMsgWrapper, MsgDCResponseUnSubscribeSymbol> EventMsgDCResponseUnSubscribeSymbol;
    public event Action<RabbitMsgWrapper, MsgSnapshotNodesSlim> EventMsgSnapshotNodesSlim;
    public event Action<RabbitMsgWrapper, MsgSnapshotDCNodesSlim> EventMsgSnapshotDCNodesSlim;
    public event Action<RabbitMsgWrapper, MsgSnapshotSingleDCNode> EventMsgSnapshotSingleDCNode;
    public event Action<RabbitMsgWrapper, MsgSnapshotGroupDCNodes> EventMsgSnapshotGroupDCNodes;

    
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
        Console.WriteLine("Sent MsgRegisterMicroService " + msg.ToString());
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
        Console.WriteLine("Sent MsgMicroServiceHealthSummaryRequest " + msg.ToString());
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
        Console.WriteLine("Sent MsgMicroServiceHealthSummaryNotification " + msg.ToString());
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
        Console.WriteLine("Sent MsgMicroServiceHealthSummarySnapshot " + msg.ToString());
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
        Console.WriteLine("Sent MsgChat " + msg.ToString());
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
        Console.WriteLine("Sent MsgTradingConnectionStatus " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSymbolStatus @event,
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
            MessageType = "MsgSymbolStatus",
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
        Console.WriteLine("Sent MsgSymbolStatus " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgStartTradingAccount @event,
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
            MessageType = "MsgStartTradingAccount",
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
        Console.WriteLine("Sent MsgStartTradingAccount " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgStopTradingAccount @event,
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
            MessageType = "MsgStopTradingAccount",
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
        Console.WriteLine("Sent MsgStopTradingAccount " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSubscribeSymbol @event,
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
            MessageType = "MsgSubscribeSymbol",
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
        Console.WriteLine("Sent MsgSubscribeSymbol " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgUnSubscribeSymbol @event,
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
            MessageType = "MsgUnSubscribeSymbol",
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
        Console.WriteLine("Sent MsgUnSubscribeSymbol " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgOpenTradeManual @event,
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
            MessageType = "MsgOpenTradeManual",
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
        Console.WriteLine("Sent MsgOpenTradeManual " + msg.ToString());
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
        Console.WriteLine("Sent MsgStartTradingAccountResponse " + msg.ToString());
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
        Console.WriteLine("Sent MsgStopTradingAccountResponse " + msg.ToString());
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
        Console.WriteLine("Sent MsgSubscribeSymbolResponse " + msg.ToString());
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
        Console.WriteLine("Sent MsgUnSubscribeSymbolResponse " + msg.ToString());
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
        Console.WriteLine("Sent MsgOpenTradeManualResponse " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgMDTick @event,
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
            MessageType = "MsgMDTick",
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
        Console.WriteLine("Sent MsgMDTick " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgMDSnapshot @event,
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
            MessageType = "MsgMDSnapshot",
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
        Console.WriteLine("Sent MsgMDSnapshot " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgMDRouting @event,
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
            MessageType = "MsgMDRouting",
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
        Console.WriteLine("Sent MsgMDRouting " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgTDTradeOpened @event,
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
            Exchange = "exchange.agent.td",
            MessageType = "MsgTDTradeOpened",
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
        Console.WriteLine("Sent MsgTDTradeOpened " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgTDTradeClosed @event,
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
            Exchange = "exchange.agent.td",
            MessageType = "MsgTDTradeClosed",
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
        Console.WriteLine("Sent MsgTDTradeClosed " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgTDSnapshot @event,
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
            Exchange = "exchange.agent.td",
            MessageType = "MsgTDSnapshot",
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
        Console.WriteLine("Sent MsgTDSnapshot " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgSyncTradingAccount @event,
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
            Exchange = "exchange.sync.trading.account",
            MessageType = "MsgSyncTradingAccount",
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
        Console.WriteLine("Sent MsgSyncTradingAccount " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgStartingDC @event,
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
            Exchange = "exchange.dc.status",
            MessageType = "MsgStartingDC",
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
        Console.WriteLine("Sent MsgStartingDC " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgStartedDC @event,
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
            Exchange = "exchange.dc.status",
            MessageType = "MsgStartedDC",
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
        Console.WriteLine("Sent MsgStartedDC " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgRestartingDC @event,
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
            Exchange = "exchange.dc.status",
            MessageType = "MsgRestartingDC",
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
        Console.WriteLine("Sent MsgRestartingDC " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgFailedToStartDC @event,
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
            Exchange = "exchange.dc.status",
            MessageType = "MsgFailedToStartDC",
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
        Console.WriteLine("Sent MsgFailedToStartDC " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCClientConnected @event,
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
            Exchange = "exchange.dc.status",
            MessageType = "MsgDCClientConnected",
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
        Console.WriteLine("Sent MsgDCClientConnected " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCClientDisconnected @event,
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
            Exchange = "exchange.dc.status",
            MessageType = "MsgDCClientDisconnected",
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
        Console.WriteLine("Sent MsgDCClientDisconnected " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCMDSnapshot @event,
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
            Exchange = "exchange.dc.md",
            MessageType = "MsgDCMDSnapshot",
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
        Console.WriteLine("Sent MsgDCMDSnapshot " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCMDTick @event,
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
            Exchange = "exchange.dc.md",
            MessageType = "MsgDCMDTick",
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
        Console.WriteLine("Sent MsgDCMDTick " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCRequestRestartFeed @event,
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
            Exchange = "exchange.dc.request",
            MessageType = "MsgDCRequestRestartFeed",
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
        Console.WriteLine("Sent MsgDCRequestRestartFeed " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCRequestStartFeed @event,
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
            Exchange = "exchange.dc.request",
            MessageType = "MsgDCRequestStartFeed",
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
        Console.WriteLine("Sent MsgDCRequestStartFeed " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCRequestStopFeed @event,
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
            Exchange = "exchange.dc.request",
            MessageType = "MsgDCRequestStopFeed",
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
        Console.WriteLine("Sent MsgDCRequestStopFeed " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCRequestSubscribeAllSymbol @event,
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
            Exchange = "exchange.dc.request",
            MessageType = "MsgDCRequestSubscribeAllSymbol",
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
        Console.WriteLine("Sent MsgDCRequestSubscribeAllSymbol " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCRequestSubscribeSymbol @event,
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
            Exchange = "exchange.dc.request",
            MessageType = "MsgDCRequestSubscribeSymbol",
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
        Console.WriteLine("Sent MsgDCRequestSubscribeSymbol " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCRequestUnSubscribeAllSymbol @event,
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
            Exchange = "exchange.dc.request",
            MessageType = "MsgDCRequestUnSubscribeAllSymbol",
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
        Console.WriteLine("Sent MsgDCRequestUnSubscribeAllSymbol " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCRequestUnSubscribeSymbol @event,
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
            Exchange = "exchange.dc.request",
            MessageType = "MsgDCRequestUnSubscribeSymbol",
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
        Console.WriteLine("Sent MsgDCRequestUnSubscribeSymbol " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCResponseRestartFeed @event,
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
            Exchange = "exchange.dc.response",
            MessageType = "MsgDCResponseRestartFeed",
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
        Console.WriteLine("Sent MsgDCResponseRestartFeed " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCResponseStartFeed @event,
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
            Exchange = "exchange.dc.response",
            MessageType = "MsgDCResponseStartFeed",
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
        Console.WriteLine("Sent MsgDCResponseStartFeed " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCResponseStopFeed @event,
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
            Exchange = "exchange.dc.response",
            MessageType = "MsgDCResponseStopFeed",
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
        Console.WriteLine("Sent MsgDCResponseStopFeed " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCResponseSubscribeAllSymbol @event,
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
            Exchange = "exchange.dc.response",
            MessageType = "MsgDCResponseSubscribeAllSymbol",
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
        Console.WriteLine("Sent MsgDCResponseSubscribeAllSymbol " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCResponseSubscribeSymbol @event,
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
            Exchange = "exchange.dc.response",
            MessageType = "MsgDCResponseSubscribeSymbol",
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
        Console.WriteLine("Sent MsgDCResponseSubscribeSymbol " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCResponseUnSubscribeAllSymbol @event,
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
            Exchange = "exchange.dc.response",
            MessageType = "MsgDCResponseUnSubscribeAllSymbol",
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
        Console.WriteLine("Sent MsgDCResponseUnSubscribeAllSymbol " + msg.ToString());
        return msg.ActionId;
    }
    
    public string Send(MsgDCResponseUnSubscribeSymbol @event,
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
            Exchange = "exchange.dc.response",
            MessageType = "MsgDCResponseUnSubscribeSymbol",
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
        Console.WriteLine("Sent MsgDCResponseUnSubscribeSymbol " + msg.ToString());
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
        Console.WriteLine("Sent MsgSnapshotNodesSlim " + msg.ToString());
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
        Console.WriteLine("Sent MsgSnapshotDCNodesSlim " + msg.ToString());
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
        Console.WriteLine("Sent MsgSnapshotSingleDCNode " + msg.ToString());
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
        Console.WriteLine("Sent MsgSnapshotGroupDCNodes " + msg.ToString());
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
            Console.WriteLine("Received MsgRegisterMicroService " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgMicroServiceHealthSummaryRequest")
        {
            EventMsgMicroServiceHealthSummaryRequest?.Invoke(msg, MessagePackSerializer.Deserialize<MsgMicroServiceHealthSummaryRequest>(msg.ByteMsg));
            Console.WriteLine("Received MsgMicroServiceHealthSummaryRequest " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgMicroServiceHealthSummaryNotification")
        {
            EventMsgMicroServiceHealthSummaryNotification?.Invoke(msg, MessagePackSerializer.Deserialize<MsgMicroServiceHealthSummaryNotification>(msg.ByteMsg));
            Console.WriteLine("Received MsgMicroServiceHealthSummaryNotification " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgMicroServiceHealthSummarySnapshot")
        {
            EventMsgMicroServiceHealthSummarySnapshot?.Invoke(msg, MessagePackSerializer.Deserialize<MsgMicroServiceHealthSummarySnapshot>(msg.ByteMsg));
            Console.WriteLine("Received MsgMicroServiceHealthSummarySnapshot " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgChat")
        {
            EventMsgChat?.Invoke(msg, MessagePackSerializer.Deserialize<MsgChat>(msg.ByteMsg));
            Console.WriteLine("Received MsgChat " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgTradingConnectionStatus")
        {
            EventMsgTradingConnectionStatus?.Invoke(msg, MessagePackSerializer.Deserialize<MsgTradingConnectionStatus>(msg.ByteMsg));
            Console.WriteLine("Received MsgTradingConnectionStatus " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSymbolStatus")
        {
            EventMsgSymbolStatus?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSymbolStatus>(msg.ByteMsg));
            Console.WriteLine("Received MsgSymbolStatus " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgStartTradingAccount")
        {
            EventMsgStartTradingAccount?.Invoke(msg, MessagePackSerializer.Deserialize<MsgStartTradingAccount>(msg.ByteMsg));
            Console.WriteLine("Received MsgStartTradingAccount " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgStopTradingAccount")
        {
            EventMsgStopTradingAccount?.Invoke(msg, MessagePackSerializer.Deserialize<MsgStopTradingAccount>(msg.ByteMsg));
            Console.WriteLine("Received MsgStopTradingAccount " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSubscribeSymbol")
        {
            EventMsgSubscribeSymbol?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSubscribeSymbol>(msg.ByteMsg));
            Console.WriteLine("Received MsgSubscribeSymbol " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgUnSubscribeSymbol")
        {
            EventMsgUnSubscribeSymbol?.Invoke(msg, MessagePackSerializer.Deserialize<MsgUnSubscribeSymbol>(msg.ByteMsg));
            Console.WriteLine("Received MsgUnSubscribeSymbol " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgOpenTradeManual")
        {
            EventMsgOpenTradeManual?.Invoke(msg, MessagePackSerializer.Deserialize<MsgOpenTradeManual>(msg.ByteMsg));
            Console.WriteLine("Received MsgOpenTradeManual " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgStartTradingAccountResponse")
        {
            EventMsgStartTradingAccountResponse?.Invoke(msg, MessagePackSerializer.Deserialize<MsgStartTradingAccountResponse>(msg.ByteMsg));
            Console.WriteLine("Received MsgStartTradingAccountResponse " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgStopTradingAccountResponse")
        {
            EventMsgStopTradingAccountResponse?.Invoke(msg, MessagePackSerializer.Deserialize<MsgStopTradingAccountResponse>(msg.ByteMsg));
            Console.WriteLine("Received MsgStopTradingAccountResponse " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSubscribeSymbolResponse")
        {
            EventMsgSubscribeSymbolResponse?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSubscribeSymbolResponse>(msg.ByteMsg));
            Console.WriteLine("Received MsgSubscribeSymbolResponse " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgUnSubscribeSymbolResponse")
        {
            EventMsgUnSubscribeSymbolResponse?.Invoke(msg, MessagePackSerializer.Deserialize<MsgUnSubscribeSymbolResponse>(msg.ByteMsg));
            Console.WriteLine("Received MsgUnSubscribeSymbolResponse " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgOpenTradeManualResponse")
        {
            EventMsgOpenTradeManualResponse?.Invoke(msg, MessagePackSerializer.Deserialize<MsgOpenTradeManualResponse>(msg.ByteMsg));
            Console.WriteLine("Received MsgOpenTradeManualResponse " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgMDTick")
        {
            EventMsgMDTick?.Invoke(msg, MessagePackSerializer.Deserialize<MsgMDTick>(msg.ByteMsg));
            Console.WriteLine("Received MsgMDTick " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgMDSnapshot")
        {
            EventMsgMDSnapshot?.Invoke(msg, MessagePackSerializer.Deserialize<MsgMDSnapshot>(msg.ByteMsg));
            Console.WriteLine("Received MsgMDSnapshot " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgMDRouting")
        {
            EventMsgMDRouting?.Invoke(msg, MessagePackSerializer.Deserialize<MsgMDRouting>(msg.ByteMsg));
            Console.WriteLine("Received MsgMDRouting " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgTDTradeOpened")
        {
            EventMsgTDTradeOpened?.Invoke(msg, MessagePackSerializer.Deserialize<MsgTDTradeOpened>(msg.ByteMsg));
            Console.WriteLine("Received MsgTDTradeOpened " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgTDTradeClosed")
        {
            EventMsgTDTradeClosed?.Invoke(msg, MessagePackSerializer.Deserialize<MsgTDTradeClosed>(msg.ByteMsg));
            Console.WriteLine("Received MsgTDTradeClosed " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgTDSnapshot")
        {
            EventMsgTDSnapshot?.Invoke(msg, MessagePackSerializer.Deserialize<MsgTDSnapshot>(msg.ByteMsg));
            Console.WriteLine("Received MsgTDSnapshot " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSyncTradingAccount")
        {
            EventMsgSyncTradingAccount?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSyncTradingAccount>(msg.ByteMsg));
            Console.WriteLine("Received MsgSyncTradingAccount " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgStartingDC")
        {
            EventMsgStartingDC?.Invoke(msg, MessagePackSerializer.Deserialize<MsgStartingDC>(msg.ByteMsg));
            Console.WriteLine("Received MsgStartingDC " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgStartedDC")
        {
            EventMsgStartedDC?.Invoke(msg, MessagePackSerializer.Deserialize<MsgStartedDC>(msg.ByteMsg));
            Console.WriteLine("Received MsgStartedDC " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgRestartingDC")
        {
            EventMsgRestartingDC?.Invoke(msg, MessagePackSerializer.Deserialize<MsgRestartingDC>(msg.ByteMsg));
            Console.WriteLine("Received MsgRestartingDC " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgFailedToStartDC")
        {
            EventMsgFailedToStartDC?.Invoke(msg, MessagePackSerializer.Deserialize<MsgFailedToStartDC>(msg.ByteMsg));
            Console.WriteLine("Received MsgFailedToStartDC " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCClientConnected")
        {
            EventMsgDCClientConnected?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCClientConnected>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCClientConnected " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCClientDisconnected")
        {
            EventMsgDCClientDisconnected?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCClientDisconnected>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCClientDisconnected " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCMDSnapshot")
        {
            EventMsgDCMDSnapshot?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCMDSnapshot>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCMDSnapshot " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCMDTick")
        {
            EventMsgDCMDTick?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCMDTick>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCMDTick " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCRequestRestartFeed")
        {
            EventMsgDCRequestRestartFeed?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCRequestRestartFeed>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCRequestRestartFeed " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCRequestStartFeed")
        {
            EventMsgDCRequestStartFeed?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCRequestStartFeed>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCRequestStartFeed " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCRequestStopFeed")
        {
            EventMsgDCRequestStopFeed?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCRequestStopFeed>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCRequestStopFeed " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCRequestSubscribeAllSymbol")
        {
            EventMsgDCRequestSubscribeAllSymbol?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCRequestSubscribeAllSymbol>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCRequestSubscribeAllSymbol " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCRequestSubscribeSymbol")
        {
            EventMsgDCRequestSubscribeSymbol?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCRequestSubscribeSymbol>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCRequestSubscribeSymbol " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCRequestUnSubscribeAllSymbol")
        {
            EventMsgDCRequestUnSubscribeAllSymbol?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCRequestUnSubscribeAllSymbol>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCRequestUnSubscribeAllSymbol " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCRequestUnSubscribeSymbol")
        {
            EventMsgDCRequestUnSubscribeSymbol?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCRequestUnSubscribeSymbol>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCRequestUnSubscribeSymbol " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCResponseRestartFeed")
        {
            EventMsgDCResponseRestartFeed?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCResponseRestartFeed>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCResponseRestartFeed " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCResponseStartFeed")
        {
            EventMsgDCResponseStartFeed?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCResponseStartFeed>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCResponseStartFeed " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCResponseStopFeed")
        {
            EventMsgDCResponseStopFeed?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCResponseStopFeed>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCResponseStopFeed " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCResponseSubscribeAllSymbol")
        {
            EventMsgDCResponseSubscribeAllSymbol?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCResponseSubscribeAllSymbol>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCResponseSubscribeAllSymbol " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCResponseSubscribeSymbol")
        {
            EventMsgDCResponseSubscribeSymbol?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCResponseSubscribeSymbol>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCResponseSubscribeSymbol " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCResponseUnSubscribeAllSymbol")
        {
            EventMsgDCResponseUnSubscribeAllSymbol?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCResponseUnSubscribeAllSymbol>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCResponseUnSubscribeAllSymbol " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgDCResponseUnSubscribeSymbol")
        {
            EventMsgDCResponseUnSubscribeSymbol?.Invoke(msg, MessagePackSerializer.Deserialize<MsgDCResponseUnSubscribeSymbol>(msg.ByteMsg));
            Console.WriteLine("Received MsgDCResponseUnSubscribeSymbol " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotNodesSlim")
        {
            EventMsgSnapshotNodesSlim?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotNodesSlim>(msg.ByteMsg));
            Console.WriteLine("Received MsgSnapshotNodesSlim " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotDCNodesSlim")
        {
            EventMsgSnapshotDCNodesSlim?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotDCNodesSlim>(msg.ByteMsg));
            Console.WriteLine("Received MsgSnapshotDCNodesSlim " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotSingleDCNode")
        {
            EventMsgSnapshotSingleDCNode?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotSingleDCNode>(msg.ByteMsg));
            Console.WriteLine("Received MsgSnapshotSingleDCNode " + msg.ToString());
            return;
        }
    
        if (msg.MessageType == "MsgSnapshotGroupDCNodes")
        {
            EventMsgSnapshotGroupDCNodes?.Invoke(msg, MessagePackSerializer.Deserialize<MsgSnapshotGroupDCNodes>(msg.ByteMsg));
            Console.WriteLine("Received MsgSnapshotGroupDCNodes " + msg.ToString());
            return;
        }
    
    }

}


    