using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DirectoryStatistic.Http.ModelData
{
    public class DataResult
    {
        public string Host { get; set; } = string.Empty;
        
        [JsonIgnore]
        public System.Net.IPAddress[] Addresses { get; set; } = Array.Empty<System.Net.IPAddress>();
        
        /// <summary>
        /// Список IPv4 адресов
        /// </summary>
        [JsonPropertyName("ipv4")]
        public string[] IPv4 { get; set; } = Array.Empty<string>();
        
        /// <summary>
        /// Список IPv6 адресов
        /// </summary>
        [JsonPropertyName("ipv6")]
        public string[] IPv6 { get; set; } = Array.Empty<string>();
        
        public long ResolveTime { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}
