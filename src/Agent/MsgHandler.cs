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
        private void HandleMsg(RabbitMsgWrapper msg, MsgStartTradingAccount @event)
        {
            try
            {
                var response = new MsgStartTradingAccountResponse()
                {
                    Ts = DateTime.UtcNow.Ticks,
                    TradingAccountId = @event.TradingAccountId,
                    TradingAccountProvider = @event.TradingAccountProvider,
                    BrokerId = @event.BrokerId,
                    Message = "Trading account started successfully.",
                    SubscribedSymbolKeys = new string[0],  // Assuming no symbols are subscribed at the start
                    FailedToSubscribeSymbolKeys = new string[0]
                };

                _engine.Connect(@event, msg.OrganizationId, msg.UserId);

                _eventGateway.Send(response, 
                    organizationId: msg.OrganizationId,
                    severity: "Info",
                    actionId: msg.ActionId,
                    userId: msg.UserId,
                    nodeId: msg.NodeId,
                    userEmail: msg.UserEmail
                );
            }
            catch (System.Exception e)
            {
                var response = new MsgStartTradingAccountResponse()
                {
                    Ts = DateTime.UtcNow.Ticks,
                    TradingAccountId = @event.TradingAccountId,
                    TradingAccountProvider = @event.TradingAccountProvider,
                    BrokerId = @event.BrokerId,
                    Message = $"Failed to start trading account: {e.Message}",
                    SubscribedSymbolKeys = new string[0],
                    FailedToSubscribeSymbolKeys = new string[0]
                };

                _eventGateway.Send(response, 
                    organizationId: msg.OrganizationId,
                    severity: "Error",
                    actionId: msg.ActionId,
                    userId: msg.UserId,
                    nodeId: msg.NodeId,
                    userEmail: msg.UserEmail
                );
            }
        }

        private void HandleMsg(RabbitMsgWrapper msg, MsgStopTradingAccount @event)
        {
            try
            {
                var response = new MsgStopTradingAccountResponse()
                {
                    Ts = DateTime.UtcNow.Ticks,
                    TradingAccountId = @event.TradingAccountId,
                    TradingAccountProvider = "Provider",
                    BrokerId = 0,
                    Message = "Trading account stopped successfully."
                };

                _engine.Disconnect(@event, msg.OrganizationId, msg.UserId);

                _eventGateway.Send(response, 
                    organizationId: msg.OrganizationId,
                    severity: "Info",
                    actionId: msg.ActionId,
                    userId: msg.UserId,
                    nodeId: msg.NodeId,
                    userEmail: msg.UserEmail
                );
            }
            catch (System.Exception e)
            {
                var response = new MsgStopTradingAccountResponse()
                {
                    Ts = DateTime.UtcNow.Ticks,
                    TradingAccountId = @event.TradingAccountId,
                    TradingAccountProvider = "prvider",
                    BrokerId = 0,
                    Message = $"Failed to stop trading account: {e.Message}"
                };

                _eventGateway.Send(response, 
                    organizationId: msg.OrganizationId,
                    severity: "Error",
                    actionId: msg.ActionId,
                    userId: msg.UserId,
                    nodeId: msg.NodeId,
                    userEmail: msg.UserEmail
                );
            }
        }

        private void HandleMsg(RabbitMsgWrapper msg, MsgSubscribeSymbol @event)
        {
            try
            {
                var response = new MsgSubscribeSymbolResponse()
                {
                    Ts = DateTime.UtcNow.Ticks,
                    TradingAccountId = @event.TradingAccountId,
                    TradingAccountProvider = "@event.TradingAccountProvider",
                    BrokerId = 0,
                    SymbolKey = @event.SymbolKey,
                    SymbolRouting = @event.SymbolRouting,
                    Digits = @event.Digits,
                    Message = "Symbol subscribed successfully."
                };

                _eventGateway.Send(response, 
                    organizationId: msg.OrganizationId,
                    severity: "Info",
                    actionId: msg.ActionId,
                    userId: msg.UserId,
                    nodeId: msg.NodeId,
                    userEmail: msg.UserEmail
                );
            }
            catch (System.Exception e)
            {
                var response = new MsgSubscribeSymbolResponse()
                {
                    Ts = DateTime.UtcNow.Ticks,
                    TradingAccountId = @event.TradingAccountId,
                    TradingAccountProvider = "Pr",
                    BrokerId = 0,
                    SymbolKey = @event.SymbolKey,
                    SymbolRouting = @event.SymbolRouting,
                    Digits = @event.Digits,
                    Message = $"Failed to subscribe symbol: {e.Message}"
                };

                _eventGateway.Send(response, 
                    organizationId: msg.OrganizationId,
                    severity: "Error",
                    actionId: msg.ActionId,
                    userId: msg.UserId,
                    nodeId: msg.NodeId,
                    userEmail: msg.UserEmail
                );
            }
        }

        private void HandleMsg(RabbitMsgWrapper msg, MsgUnSubscribeSymbol @event)
        {
            try
            {
                var response = new MsgUnSubscribeSymbolResponse()
                {
                    Ts = DateTime.UtcNow.Ticks,
                    TradingAccountId = @event.TradingAccountId,
                    TradingAccountProvider = "fsf",
                    BrokerId = 0,
                    SymbolKey = @event.SymbolKey,
                    SymbolRouting = "Routing",
                    Digits = 0,
                    Message = "Symbol unsubscribed successfully."
                };

                _eventGateway.Send(response, 
                    organizationId: msg.OrganizationId,
                    severity: "Info",
                    actionId: msg.ActionId,
                    userId: msg.UserId,
                    nodeId: msg.NodeId,
                    userEmail: msg.UserEmail
                );
            }
            catch (System.Exception e)
            {
                var response = new MsgUnSubscribeSymbolResponse()
                {
                    Ts = DateTime.UtcNow.Ticks,
                    TradingAccountId = @event.TradingAccountId,
                    TradingAccountProvider = "Fefe",
                    BrokerId = 0,
                    SymbolKey = @event.SymbolKey,
                    SymbolRouting = "Routing",
                    Digits = 0,
                    Message = $"Failed to unsubscribe symbol: {e.Message}"
                };

                _eventGateway.Send(response, 
                    organizationId: msg.OrganizationId,
                    severity: "Error",
                    actionId: msg.ActionId,
                    userId: msg.UserId,
                    nodeId: msg.NodeId,
                    userEmail: msg.UserEmail
                );
            }
        }
    }
}
