using HFTbridge.Node.Shared.Services;
using HFTbridge.Msg;

namespace HFTbridge.Node.DC
{
    public class MsgHandler
    {
        private EventGateway _eventGateway;
        public MsgHandler(EventGateway eventGateway)
        {
            _eventGateway = eventGateway;
            _eventGateway.EventMsgDCRequestRestartFeed += HandleMsg;
            _eventGateway.EventMsgDCRequestStartFeed += HandleMsg;
            _eventGateway.EventMsgDCRequestStopFeed += HandleMsg;
            _eventGateway.EventMsgDCRequestSubscribeAllSymbol += HandleMsg;
            _eventGateway.EventMsgDCRequestSubscribeSymbol += HandleMsg;
            _eventGateway.EventMsgDCRequestUnSubscribeAllSymbol += HandleMsg;
            _eventGateway.EventMsgDCRequestUnSubscribeSymbol += HandleMsg;
        }

        // Handlers
        private void HandleMsg (RabbitMsgWrapper msg, MsgDCRequestRestartFeed @event)
        {
            Console.WriteLine("MsgDCRequestRestartFeed");
            
        }

        private void HandleMsg (RabbitMsgWrapper msg, MsgDCRequestStartFeed @event)
        {
            Console.WriteLine("MsgDCRequestStartFeed");
        }

        private void HandleMsg (RabbitMsgWrapper msg, MsgDCRequestStopFeed @event)
        {
            Console.WriteLine("MsgDCRequestStopFeed");
        }

        private void HandleMsg (RabbitMsgWrapper msg, MsgDCRequestSubscribeAllSymbol @event)
        {
            Console.WriteLine("MsgDCRequestSubscribeAllSymbol");
        }

        private void HandleMsg (RabbitMsgWrapper msg, MsgDCRequestSubscribeSymbol @event)
        {
            Console.WriteLine("MsgDCRequestSubscribeSymbol");
        }

        private void HandleMsg (RabbitMsgWrapper msg, MsgDCRequestUnSubscribeAllSymbol @event)
        {
            Console.WriteLine("MsgDCRequestUnSubscribeAllSymbol");
        }

        private void HandleMsg (RabbitMsgWrapper msg, MsgDCRequestUnSubscribeSymbol @event)
        {
            Console.WriteLine("MsgDCRequestUnSubscribeSymbol");
        }

    }
}
