using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Botmito
{
    public struct FactJson
    {
        [JsonPropertyName("fact")]
        public string Fact { get; set; }
    }
}
