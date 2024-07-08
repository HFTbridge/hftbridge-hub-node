// namespace HFTbridge.Agent;
// public class ReceiveMessagesService
// {
//     private readonly IHftBridgeEngine _hftBridgeEngine;
//     private readonly ILogger _logger;

//     public ReceiveMessagesService(ILogger logger, IHftBridgeEngine hftBridgeEngine)
//     {
//         _logger = logger;
//         _hftBridgeEngine = hftBridgeEngine;
//     }

//     public void OnEvent(string eventName, byte[] @event)
//     {
//         try
//         {
//             string typeName = $"HFTbridge.Agent.Messages.{eventName}";
//             string assemblyName = typeof(MessageRequestStartTradingConnection).Assembly.FullName;
//             string assemblyQualifiedName = $"{typeName}, {assemblyName}";

//             Type eventType = Type.GetType(assemblyQualifiedName);
//             if (eventType == null)
//             {
//                 _logger.Error("Type not found for event name.");
//                 return;
//             }

//             MethodInfo deserializeMethod = typeof(MessagePackSerializer).GetMethod(nameof(MessagePackSerializer.Deserialize), new[] { typeof(ReadOnlyMemory<byte>), typeof(MessagePackSerializerOptions), typeof(CancellationToken) });
//             if (deserializeMethod == null)
//             {
//                 _logger.Error("Deserialize method not found.");
//                 return;
//             }

//             MethodInfo genericDeserializeMethod = deserializeMethod.MakeGenericMethod(eventType);
//             object deserializedEvent = genericDeserializeMethod.Invoke(null, new object[] { new ReadOnlyMemory<byte>(@event), MessagePackSerializerOptions.Standard, CancellationToken.None });

//             MethodInfo eventHandlerMethod = this.GetType().GetMethod("OnEvent", new Type[] { eventType });
//             if (eventHandlerMethod == null)
//             {
//                 _logger.Error("Event handler method not found for event type.");
//                 return;
//             }

//             eventHandlerMethod.Invoke(this, new object[] { deserializedEvent });

//         }
//         catch (Exception ex)
//         {
//             _logger.Error("An exception occurred during deserialization or event handling: {error}" + ex);
//         }
//     }

// //-----------> Request Handlers <----------
//     public void OnEvent(MessageRequestStartTradingConnection @event)
//     {
//         _hftBridgeEngine.Connection2.Connect(@event.TradingConnectionId, @event.ConneectionString, @event.TypeLabel);
//         if (@event.SymbolKeys.Length == 0)
//             return;
//         _hftBridgeEngine.Connection2.Subscribe(@event.TradingConnectionId, @event.SymbolKeys);
//     }

//     public void OnEvent( MessageRequestStopTradingConnection @event)
//     {
//         _hftBridgeEngine.Connection2.Disconnect(@event.TradingConnectionId);
//     }

//     public void OnEvent( MessageRequestSubscribeTradingConnectionMd @event)
//     {
//         _hftBridgeEngine.Connection2.Subscribe(@event.TradingConnectionId, @event.SymbolKeys);
//     }

//     public void OnEvent( MessageRequestUnSubscribeTradingConnectionMd @event)
//     {
//         _hftBridgeEngine.Connection2.UnSubscribe(@event.TradingConnectionId, @event.SymbolKeys);
//     }

//     public void OnEvent( MessageRequestOpenManualTrade @event)
//     {
//         _hftBridgeEngine.Strategy2.OpenManualTradeSignal(@event);
//     }

//     public void OnEvent( MessageRequestStartStrategy @event)
//     {
//         _hftBridgeEngine.Strategy2.StartStrategy(@event);
//     }

//     public void OnEvent( MessageRequestStopStrategy @event)
//     {
//         _hftBridgeEngine.Strategy2.StopStrategy(@event);
//     }

//     public void OnEvent( MessageRequestStartSymbolStrategySetup @event)
//     {
//         _hftBridgeEngine.Strategy2.StartSetup(@event);
//     }

//     public void OnEvent( MessageRequestStopSymbolStrategySetup @event)
//     {
//         _hftBridgeEngine.Strategy2.StopSetup(@event);
//     }


// }
