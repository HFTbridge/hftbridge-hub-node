using System;
using System.Threading;

namespace HFTbridge.Node.Shared.Services
{
    public class SyncWorker
    {
        private Timer _timer;

        private readonly  EventGateway _eventGateway;

        private readonly ISyncWorkerHandler _syncWorkerHandler;

        private readonly string _os;
        private readonly string _countryCode;
        

        public SyncWorker(EventGateway eventGateway, ISyncWorkerHandler syncWorkerHandler, string os, string countryCode)
        {
            _os = os;
            _countryCode = countryCode;
            _syncWorkerHandler = syncWorkerHandler;
            _eventGateway = eventGateway;
            _timer = new Timer(TimerCallback, null, 0, 1000);
        }

        private void TimerCallback(object state)
        {
            try
            {
                _syncWorkerHandler.OnEverySecond(_eventGateway, _os, _countryCode);
            }
            catch (System.Exception e)
            {
                
                Log.Logger.Error("Faile to send full snapshot !!! {@msg}", e.Message);
            }
            
        }
    }
}
