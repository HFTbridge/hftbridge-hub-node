using System;
using System.Net.Sockets;
using System.Threading;
using Serilog;
using TcpClient = NetCoreServer.TcpClient;

using com.chronoxor.simple;
using com.chronoxor.simple.FBE;
using System.Collections.Generic;

namespace ProtoClient
{
    public class TcpProtoClient : TcpClient
    {
        public TcpProtoClient(string address, int port) : base(address, port)
        {
            LoggingConfigurator.ConfigureLogger();
        }

        public bool ConnectAndStart()
        {
            Log.Information("TCP protocol client starting a new session with Id '{Id}'", Id);
            StartReconnectTimer();
            return ConnectAsync();
        }

        public bool DisconnectAndStop()
        {
            Log.Information("TCP protocol client stopping the session with Id '{Id}'", Id);
            StopReconnectTimer();
            DisconnectAsync();
            return true;
        }

        public override bool Reconnect()
        {
            return ReconnectAsync();
        }

        private Timer _reconnectTimer;

        public void StartReconnectTimer()
        {
            // Start the reconnect timer
            _reconnectTimer = new Timer(state =>
            {
                Log.Information("TCP reconnect timer connecting the client session with Id '{Id}'", Id);
                ConnectAsync();
            }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void StopReconnectTimer()
        {
            // Stop the reconnect timer
            _reconnectTimer?.Dispose();
            _reconnectTimer = null;
        }

        public delegate void ConnectedHandler();
        public event ConnectedHandler Connected = () => {};

        protected override void OnConnected()
        {
            Log.Information("TCP protocol client connected a new session with Id '{Id}' to remote address '{Address}' and port {Port}", Id, Address, Port);
            Connected?.Invoke();
        }

        public delegate void DisconnectedHandler();
        public event DisconnectedHandler Disconnected = () => {};

        protected override void OnDisconnected()
        {
            Log.Information("TCP protocol client disconnected the session with Id '{Id}'", Id);
            // Setup and asynchronously wait for the reconnect timer
            _reconnectTimer?.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);

            Disconnected?.Invoke();
        }

        public delegate void ReceivedHandler(byte[] buffer, long offset, long size);
        public event ReceivedHandler Received = (buffer, offset, size) => {};

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            Received?.Invoke(buffer, offset, size);
        }

        protected override void OnError(SocketError error)
        {
            Log.Error("TCP protocol client caught a socket error: {error}", error);
        }

        #region IDisposable implementation

        // Disposed flag.
        private bool _disposed;

        protected override void Dispose(bool disposingManagedResources)
        {
            if (!_disposed)
            {
                if (disposingManagedResources)
                {
                    // Dispose managed resources here...
                    StopReconnectTimer();
                }

                // Dispose unmanaged resources here...

                // Set large fields to null here...

                // Mark as disposed.
                _disposed = true;
            }

            // Call Dispose in the base class.
            base.Dispose(disposingManagedResources);
        }

        // The derived class does not have a Finalize method
        // or a Dispose method without parameters because it inherits
        // them from the base class.

        #endregion
    }

    public class SimpleProtoClient : Client, ISenderListener, IReceiverListener, IDisposable
    {
        private readonly TcpProtoClient _tcpProtoClient;

        public Guid Id => _tcpProtoClient.Id;
        public bool IsConnected => _tcpProtoClient.IsConnected;
        public List<string> symbols = new List<string>();

        public event EventHandler<NewMdNotify> NewTick;



        public SimpleProtoClient(string address, int port)
        {
            LoggingConfigurator.ConfigureLogger();
            _tcpProtoClient = new TcpProtoClient(address, port);
            _tcpProtoClient.Connected += OnConnected;
            _tcpProtoClient.Disconnected += OnDisconnected;
            _tcpProtoClient.Received += OnReceived;
            ReceivedResponse_DisconnectRequest += HandleDisconnectRequest;
            ReceivedResponse_SimpleResponse += HandleSimpleResponse;
            ReceivedResponse_SimpleReject += HandleSimpleReject;
            ReceivedResponse_SimpleNotify += HandleSimpleNotify;
            ReceivedResponse_NewMdNotify += HandleNewMdNtofiy;
            ReceivedResponse_GetSymbolResponse += HandleSymbolResponse;
        }

        private void DisposeClient()
        {
            _tcpProtoClient.Connected -= OnConnected;
            _tcpProtoClient.Connected -= OnDisconnected;
            _tcpProtoClient.Received -= OnReceived;
            ReceivedResponse_DisconnectRequest -= HandleDisconnectRequest;
            ReceivedResponse_SimpleResponse -= HandleSimpleResponse;
            ReceivedResponse_SimpleReject -= HandleSimpleReject;
            ReceivedResponse_SimpleNotify -= HandleSimpleNotify;
            ReceivedResponse_NewMdNotify -= HandleNewMdNtofiy;
            ReceivedResponse_GetSymbolResponse -= HandleSymbolResponse;
            _tcpProtoClient.Dispose();
        }

        public bool ConnectAndStart() { return _tcpProtoClient.ConnectAndStart(); }
        public bool DisconnectAndStop() { return _tcpProtoClient.DisconnectAndStop(); }
        public bool Reconnect() { return _tcpProtoClient.Reconnect(); }

        private bool _watchdog;
        private Thread _watchdogThread;

        public bool StartWatchdog()
        {
            if (_watchdog)
                return false;

            Log.Information("Watchdog thread starting...");


            // Start the watchdog thread
            _watchdog = true;
            _watchdogThread = new Thread(WatchdogThread);

            Log.Information("Watchdog thread started!");

            return true;
        }

        public bool StopWatchdog()
        {
            if (!_watchdog)
                return false;

            Log.Information("Watchdog thread stopping...");

            // Stop the watchdog thread
            _watchdog = false;
            _watchdogThread.Join();

            Log.Information("Watchdog thread stopped!");

            return true;
        }

        public static void WatchdogThread(object obj)
        {
            var instance = obj as SimpleProtoClient;
            if (instance == null)
                return;

            try
            {
                // Watchdog loop...
                while (instance._watchdog)
                {
                    var utc = DateTime.UtcNow;

                    // Watchdog the client
                    instance.Watchdog(utc);

                    // Sleep for a while...
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                Log.Error("Config client watchdog thread terminated: {e}", e.Message);
            }
        }

        #region Connection handlers

        public delegate void ConnectedHandler();
        public event ConnectedHandler Connected = () => {};

        private void OnConnected()
        {
            // Reset FBE protocol buffers
            Reset();

            Connected?.Invoke();
        }

        public delegate void DisconnectedHandler();
        public event DisconnectedHandler Disconnected = () => {};

        private void OnDisconnected()
        {
            Disconnected?.Invoke();
        }

        public long OnSend(byte[] buffer, long offset, long size)
        {
            return _tcpProtoClient.SendAsync(buffer, offset, size) ? size : 0;
        }

        public void OnReceived(byte[] buffer, long offset, long size)
        {
            Receive(buffer, offset, size);
        }

        #endregion

        #region Protocol handlers

        private void HandleDisconnectRequest(DisconnectRequest request) { Console.WriteLine($"Received: {request}"); _tcpProtoClient.DisconnectAsync(); }
        private void HandleSimpleResponse(SimpleResponse response) { Console.WriteLine($"Received: {response}"); }
        private void HandleSimpleReject(SimpleReject reject) { Console.WriteLine($"Received: {reject}"); }
        private void HandleSimpleNotify(SimpleNotify notify) { Console.WriteLine($"Received: {notify}"); }
        private void HandleNewMdNtofiy(NewMdNotify notify) 
        { 
            // EventHandler<NewMdNotify> handler = NewTick;
            // handler?.Invoke(this, notify);
        }
        private void HandleSymbolResponse(GetSymbolResponse  symbolResponse) { symbols = symbolResponse.Symbols; }
        

        #endregion

        #region IDisposable implementation

        // Disposed flag.
        private bool _disposed;

        // Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposingManagedResources)
        {
            // The idea here is that Dispose(Boolean) knows whether it is
            // being called to do explicit cleanup (the Boolean is true)
            // versus being called due to a garbage collection (the Boolean
            // is false). This distinction is useful because, when being
            // disposed explicitly, the Dispose(Boolean) method can safely
            // execute code using reference type fields that refer to other
            // objects knowing for sure that these other objects have not been
            // finalized or disposed of yet. When the Boolean is false,
            // the Dispose(Boolean) method should not execute code that
            // refer to reference type fields because those objects may
            // have already been finalized."

            if (!_disposed)
            {
                if (disposingManagedResources)
                {
                    // Dispose managed resources here...
                    DisposeClient();
                }

                // Dispose unmanaged resources here...

                // Set large fields to null here...

                // Mark as disposed.
                _disposed = true;
            }
        }

        #endregion
    }
}
