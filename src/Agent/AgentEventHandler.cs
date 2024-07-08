// namespace HFTbridge.Agent
// {
//     public class AgentEventHandler
//     {
//         private readonly ILogger _logger;
//         private readonly HubConnection _hubConnection;

//         private ReceiveMessagesService _receiveMessagesService;

//         private readonly IHftBridgeEngine _hftBridgeEngine;
        
//         public AgentEventHandler(
//             ILogger logger,
//             IHftBridgeEngine hftBridgeEngine,
//             HubConnection hubConnection, 
//             ReceiveMessagesService receiveMessagesService
//             )
//         {
//             _logger = logger;
//             _receiveMessagesService = receiveMessagesService;
//             _hftBridgeEngine = hftBridgeEngine;
//             _hubConnection = hubConnection;

//             _hftBridgeEngine.Events.OnMessageEvent += OnEvent;
            
//             _hubConnection.On("HandleNewMessage",
//                 ( string eventName, byte[] eventData) =>
//                     ReceiveMessage( eventName, eventData));
//         }

//         private void ReceiveMessage(string eventName, byte[] @event)
//         {
//             _logger.Information("Received: {event}", eventName);
//             _receiveMessagesService.OnEvent( eventName,  @event);
//         }

//         private async void OnEvent(object sender, object @event)
//         {
//             try
//             {
//                 var eventType = @event.GetType();
//                 var messageType = eventType.Name;

                
//                 string routingKey = null;

//                 if (messageType.Contains("MessageStatus"))
//                 {
//                     routingKey = "STATUS";
//                 }

//                 if (messageType.Contains("MessageMd"))
//                 {
//                     routingKey = "MD";
//                 }

//                 // Use the extracted routing key or a default value
//                 var routingKeyValue = routingKey ?? "ERROR";

//                 //Console.WriteLine(routingKeyValue);

//                 var serialized = MessagePackSerializer.Serialize(@event);

//                 await _hubConnection.InvokeAsync("PublishMessage",routingKey, messageType, serialized);
//                 _logger.Information( "Published: {MessageType}", @event.GetType().Name);

//             }
//             catch (Exception ex)
//             {
//                 _logger.Error(ex, "Error publishing message of type {MessageType}", @event.GetType());
//             }
//         }
//     }
// }
