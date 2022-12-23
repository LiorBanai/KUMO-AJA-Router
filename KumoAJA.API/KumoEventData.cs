using Newtonsoft.Json;

namespace Kumo.Routing.API
{
    public class KumoEventData
    {
        [JsonProperty("param_id")] public string ParamID { get; set; }
        [JsonProperty("param_type")] public string ParamType { get; set; }
        [JsonProperty("int_value")] public int NumericValue { get; set; }

        public string NameValue => StrValue.ToString();

        [JsonProperty("str_value")] public object StrValue { get; set; }
        [JsonProperty("last_config_update")] public string ConfigUpdate { get; set; }

        public override string ToString()
        {
            return $"{nameof(ParamID)}: {ParamID}, {nameof(ParamType)}: {ParamType}, {nameof(NumericValue)}: {NumericValue}, {nameof(NameValue)}: {NameValue}";
        }
    }
}

