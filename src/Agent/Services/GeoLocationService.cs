using System;
using System.Net.Http;
using System.Text.Json;

namespace HFTbridge.Agent.Services
{
    public class GeoLocationManager
    {
        private readonly string _ipInfoApiUrl = "https://ipinfo.io/json";

        public GeoLocationEntry Data {get;}

        public GeoLocationManager()
        {
            Log.Logger.Debug("Fetching The Machine Geo Location");
            try
            {
                Data = GetGeoLocation();
                Log.Logger.Debug("Geo location for this machine: {@data}", Data);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Failed to download machine geo location", ex);
                throw;
            }
        }

        private GeoLocationEntry GetGeoLocation()
        {
            using var client = new HttpClient();
            var response = client.GetStringAsync(_ipInfoApiUrl).Result;
            if (string.IsNullOrEmpty(response))
            {
                throw new Exception("Unable to fetch geo location from the API");
            }

            var geoLocation = JsonSerializer.Deserialize<GeoLocationEntry>(response);
            return geoLocation;
        }
    }

    public class GeoLocationEntry
    {
        public string ip { get; set; }
        public string hostname { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public string loc { get; set; }
        public string org { get; set; }
        public string postal { get; set; }
        public string timezone { get; set; }
    }
}
