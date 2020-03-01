using System;
using Newtonsoft.Json;
// ReSharper disable UnusedMember.Global

namespace MoscowMeetupBot.Models
{
    public class EventItem
    {
        public int Id { get; set; }
        
        [JsonProperty("starts_at")]
        public DateTime startsAt { get; set; }
        
        public string Name { get; set; }
        [JsonProperty("description_short")]
        public string DescriptionShort { get; set; }

        public Uri Url { get; set; }
        
        [JsonProperty("poster_image")]
        public PosterImageItem PosterImage { get; set; }
        
        public LocationItem Location { get; set; }
    }
}