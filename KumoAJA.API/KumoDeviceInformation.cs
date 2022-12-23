using Newtonsoft.Json;

namespace Kumo.Routing.API
{
    internal class KumoDeviceInformation
    {
        [JsonProperty("service_description")] public string ServiceDescription { get; set; }
        [JsonProperty("description")] public string Description { get; set; }


    }
}