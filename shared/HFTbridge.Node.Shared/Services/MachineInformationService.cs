using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hardware.Info;

namespace HFTbridge.Node.Shared.Services
{

    public class MachineInformationService
    {
        public HardwareInfo Data {get;set;}
        public MachineInformationService()
        {
            Data = new HardwareInfo();
            Data.RefreshAll();
            Log.Logger.Debug("Machine information loaded: {@data}", Data);
        }

        public string GetHardwareInformationJson()
        {
            string json = string.Empty;
            try
            {
                Data.RefreshAll();

                // Configure JsonSerializerOptions to handle IPAddress serialization
                var options = new JsonSerializerOptions
                {
                    Converters = { new IPAddressConverter() },
                    WriteIndented = true // optional: makes the output JSON indented for better readability
                };

                // Serialize the hardwareInfo object to JSON
                json = JsonSerializer.Serialize(Data, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return json;
        }

       
    }

    public class IPAddressConverter : JsonConverter<IPAddress>
    {
        public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(); // Deserialization is not implemented in this example
        }

        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}