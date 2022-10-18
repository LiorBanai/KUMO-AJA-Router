using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KumoAJA.API
{
    internal class KumoConnectionIdData
    {
        [JsonProperty("connectionid")] public int ConnectionId { get; set; }
    }
}
