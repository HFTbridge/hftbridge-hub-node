using System;
using System.Threading;

namespace HFTbridge.Node.Shared.Services
{
    public class SyncWorker
    {
        private Timer _timer;

        private readonly  EventGateway _eventGateway;

        public SyncWorker(EventGateway eventGateway)
        {
            _eventGateway = eventGateway;
            _timer = new Timer(TimerCallback, null, 0, 1000);
        }

        private void TimerCallback(object state)
        {
            Console.WriteLine("Message logged at: " + DateTime.Now);
        }
    }
}
