// using HFTbridge.Node.Shared.Services;
using HFTbridge.Msg;

namespace HFTbridge.Node.Agent
{
    public class MsgHandler
    {
        private EventGateway _eventGateway;
        private readonly HFTBridgeEngine _engine;
        public MsgHandler(EventGateway eventGateway, HFTBridgeEngine engine)
        {
            _engine = engine;
            _eventGateway = eventGateway;
            _eventGateway.EventMsgStartTradingAccount += HandleMsg;
            _eventGateway.EventMsgStopTradingAccount += HandleMsg;
            _eventGateway.EventMsgSubscribeSymbol += HandleMsg;
            _eventGateway.EventMsgUnSubscribeSymbol += HandleMsg;

        }

        // Handlers
        private void HandleMsg (RabbitMsgWrapper msg, MsgStartTradingAccount @event)
        {
            try
            {
                var response = new MsgStartTradingAccountResponse()
                {
                    Ts = DateTime.UtcNow.Ticks,
                    ServiceName = "fefefe",
                    ServiceUrl = "efwfewfw",
                    ServiceVersion = "hbfewbifwefbw",
                };

                _engine.Connect(@event, msg.OrganizationId, msg.UserId);

                _eventGateway.Send(response, 
                    organizationId:msg.OrganizationId,
                    severity:"Warning",
                    actionId:msg.ActionId,
                    userId:msg.UserId,
                    nodeId:msg.NodeId,
                    userEmail:msg.UserEmail
                );
            }
            catch (System.Exception e)
            {
                var response = new MsgStartTradingAccountResponse()
                {
                    Ts = DateTime.UtcNow.Ticks,
                    ServiceName = "fefefe",
                    ServiceUrl = "efwfewfw",
                    ServiceVersion = "hbfewbifwefbw",
                };

                _eventGateway.Send(response, 
                    organizationId:msg.OrganizationId,
                    severity:"Warning",
                    actionId:msg.ActionId,
                    userId:msg.UserId,
                    nodeId:msg.NodeId,
                    userEmail:msg.UserEmail
                );
            }

            
        }

        private void HandleMsg (RabbitMsgWrapper msg, MsgStopTradingAccount @event)
        {
            try
            {
                var response = new MsgStopTradingAccountResponse()
                {
                    Ts = DateTime.UtcNow.Ticks,
                    ServiceName = "fefefe",
                    ServiceUrl = "efwfewfw",
                    ServiceVersion = "hbfewbifwefbw",
                };
                
                _engine.Disconnect(@event, msg.OrganizationId, msg.UserId);

                _eventGateway.Send(response, 
                    organizationId:msg.OrganizationId,
                    severity:"Warning",
                    actionId:msg.ActionId,
                    userId:msg.UserId,
                    nodeId:msg.NodeId,
                    userEmail:msg.UserEmail
                );
            }
            catch (System.Exception e)
            {
                var response = new MsgStopTradingAccountResponse()
                {
                    Ts = DateTime.UtcNow.Ticks,
                    ServiceName = "fefefe",
                    ServiceUrl = "efwfewfw",
                    ServiceVersion = "hbfewbifwefbw",
                };
                
                _eventGateway.Send(response, 
                    organizationId:msg.OrganizationId,
                    severity:"Warning",
                    actionId:msg.ActionId,
                    userId:msg.UserId,
                    nodeId:msg.NodeId,
                    userEmail:msg.UserEmail
                );
            }
            
            
        }

         private void HandleMsg (RabbitMsgWrapper msg, MsgSubscribeSymbol @event)
        {
            var response = new MsgSubscribeSymbolResponse()
            {
                Ts = DateTime.UtcNow.Ticks,
                ServiceName = "fefefe",
                ServiceUrl = "efwfewfw",
                ServiceVersion = "hbfewbifwefbw",
            };

            _eventGateway.Send(response, 
                organizationId:msg.OrganizationId,
                severity:"Warning",
                actionId:msg.ActionId,
                userId:msg.UserId,
                nodeId:msg.NodeId,
                userEmail:msg.UserEmail
            );
            
        }

        private void HandleMsg (RabbitMsgWrapper msg, MsgUnSubscribeSymbol @event)
        {
            var response = new MsgUnSubscribeSymbolResponse()
            {
                Ts = DateTime.UtcNow.Ticks,
                ServiceName = "fefefe",
                ServiceUrl = "efwfewfw",
                ServiceVersion = "hbfewbifwefbw",
            };
            
            _eventGateway.Send(response, 
                organizationId:msg.OrganizationId,
                severity:"Warning",
                actionId:msg.ActionId,
                userId:msg.UserId,
                nodeId:msg.NodeId,
                userEmail:msg.UserEmail
            );
            
        }

    

    }
}
