namespace HFTbridge.Node.Shared
{
    public interface ISyncWorkerHandler
    {
        public void OnEverySecond(EventGateway eventGateway, string os, string countryCode);
    }
}