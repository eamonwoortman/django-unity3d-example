using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Score {
    public int Id;

    [JsonProperty("score")]
    public int Amount;

    public DateTime Updated;
}
