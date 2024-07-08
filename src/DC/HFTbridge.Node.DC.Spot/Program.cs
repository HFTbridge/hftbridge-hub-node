using HFTbridge.Node.Shared.Services;

namespace HFTbridge.Node.DC
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var mothershipService = new MothershipService("https://hub.dc.hft-app.net", "SPOT", "PUBLIC");
            mothershipService.Start();


            while (true)
            {
                await Task.Delay(1000);
            }
        }

    }
}
