using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DirectoryStatistic
{
    public class DeserializeZaprosClass
    {
            [JsonPropertyName("ip")]
            public string ip { get; set; }

            [JsonPropertyName("hostname")]
            public string hostname { get; set; }

            [JsonPropertyName("city")]
            public string city { get; set; }

            [JsonPropertyName("region")]
            public string region { get; set; }

            [JsonPropertyName("country")]
            public string country { get; set; }

            [JsonPropertyName("loc")]
            public string loc { get; set; }

            [JsonPropertyName("org")]
            public string org { get; set; }

            [JsonPropertyName("postal")]
            public string postal { get; set; }

            [JsonPropertyName("timezone")]
            public string timezone { get; set; }
    }
}
