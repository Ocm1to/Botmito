using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Botmito
{
    public struct ConfigJson
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("prefix")]
        public string Prefix { get; set; }
    }
}
