using System;
using Newtonsoft.Json;

namespace MoscowMeetupBot.Models
{
    public class PosterImageItem
    {
        [JsonProperty("default_url")]
        public Uri DefaultUrl { get; set; }
    }
}