// using HFTbridge.Node.Shared.Services;
using HFTbridge.Msg;

namespace HFTbridge.Node.Agent
{
    public class MsgHandler
    {
        private EventGateway _eventGateway;
        private readonly HFTBridgeEngine _engine;
        private readonly string _nodeId;
        private readonly string _organizationId;

        public MsgHandler(EventGateway eventGateway, HFTBridgeEngine engine, string nodeId, string organizationId)
        {
            _engine = engine;
            _eventGateway = eventGateway;
            _nodeId = nodeId;
            _organizationId = organizationId;

            _eventGateway.EventMsgStartTradingAccountRequest += HandleMsg;
            _eventGateway.EventMsgStopTradingAccountRequest += HandleMsg;
            _eventGateway.EventMsgSubscribeSymbolRequest += HandleMsg;
            _eventGateway.EventMsgUnSubscribeSymbolRequest += HandleMsg;

            _engine.OnMsgMDRoutingBulk += HandleMsg;
            _engine.OnMsgTCLog += HandleMsg;
        }

        // Handlers
        private void HandleMsg(RabbitMsgWrapper msg, MsgStartTradingAccountRequest @event)
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

        private void HandleMsg(RabbitMsgWrapper msg, MsgStopTradingAccountRequest @event)
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

        private void HandleMsg(RabbitMsgWrapper msg, MsgSubscribeSymbolRequest @event)
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

        private void HandleMsg(RabbitMsgWrapper msg, MsgUnSubscribeSymbolRequest @event)
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

        private void HandleMsg(MsgMDRoutingBulk @event)
        {
            try
            {
                _eventGateway.Send(@event, 
                    organizationId: _organizationId,
                    severity: "Debug",
                    actionId: "MDBULK",
                    userId: "System",
                    nodeId: _nodeId,
                    userEmail: "System"
                );
            }
            catch (System.Exception e)
            {
                Console.WriteLine("ERROR PUBLISHING MARKET DATA TO MOTHERSHIP !!!!!!!!!");
            }
        }

        private void HandleMsg(MsgTCLog @event, string organizationId, string userId)
        {
            try
            {
                _eventGateway.Send(@event, 
                    organizationId: organizationId,
                    severity: "Debug",
                    actionId: "TCLOG",
                    userId: userId,
                    nodeId: _nodeId,
                    userEmail: "System"
                );
                Console.WriteLine("TCLOG --- " + @event.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("ERROR PUBLISHING TCLOG TO MOTHERSHIP !!!!!!!!!");
            }
        }
    }
}
