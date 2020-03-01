using System;
using System.Threading.Tasks;
using MoscowMeetupBot.Models;
using RestEase;

namespace MoscowMeetupBot
{
    public interface ITimepadApi
    {
        [Get("/v1/events")]
        Task<GetEventsData> GetEvents(string[] fields, [Query("category_ids")] int[] categoryIds, string[] cities, [Query("created_at_min")] DateTime createdAtMin);

        [Get("/v1/events/{eventId}")]
        Task<EventItem> GetEvent([Path] int eventId, string[] fields = null);

    }
}