using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Botmito
{
    public struct JokeJson
    {
        [JsonPropertyName("joke")]
        public string Joke { get; set; }
    }
}
