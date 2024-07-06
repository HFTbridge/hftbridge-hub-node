
// MsgSchema.cs
global using MessagePack;
global using HFTbridge.Msg;
namespace HFTbridge.Msg;

public static class MsgSchema
{
    public static int Version = 3;
}
    

[MessagePackObject]
public record struct MsgRegisterMicroService(
    [property: Key(0)] long Ts,
    [property: Key(1)] string ServiceName,
    [property: Key(2)] string ServiceUrl,
    [property: Key(3)] string ServiceVersion
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
    [property: Key(4)] SubMsgStartTradingAccountSubscribeItem[] Subscribe
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
    