using System.Collections.Generic;
using Newtonsoft.Json;

namespace DRFV.JsonData
{
    public class Songlist
    {
        [JsonProperty("songs")] public List<SonglistItem> songs = new();
    }

    public class SonglistItem
    {
        [JsonProperty("keyword")] public string keyword = "";

        [JsonProperty("name")] public string name = "";

        [JsonProperty("artist")] public string artist = "";

        [JsonProperty("bpm")] public string bpm = "";

        [JsonProperty("preview")] public float preview = -1;
    }
}