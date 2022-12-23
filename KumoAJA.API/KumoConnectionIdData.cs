using Newtonsoft.Json;

namespace Kumo.Routing.API
{
    internal class KumoConnectionIdData
    {
        [JsonProperty("connectionid")] public int ConnectionId { get; set; }
    }
}