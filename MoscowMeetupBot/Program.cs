using System;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Extensions.Configuration;
using MoscowMeetupBot.Models;
using Nett.AspNet;
using Newtonsoft.Json;
using RestEase;
using Serilog;
using Serilog.Core;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace MoscowMeetupBot
{
    class Program
    {
        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .AddTomlFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.toml"), false)
            .Build();
        
        private static async Task Main()
        {
            try
            {
                var (botConfig, telegramConfig) = (Configuration.GetSection("MoscowMeetupBot").Get<MoscowMeetupBotConfiguration>(), Configuration.GetSection("Telegram").Get<TelegramConfiguration>());
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(Configuration, "Serilog")
                    .CreateLogger();

                Log.Information("Staring...");
                var checkLog = JsonConvert.DeserializeObject<CheckLog>(File.ReadAllText("checklog.json"));

                var telegramClient = new TelegramBotClient(telegramConfig.BotToken);
                var timepadClient = RestClient.For<ITimepadApi>("https://api.timepad.ru");
                
                Log.Information("Getting information about events from {Date}", checkLog.CheckTime);
                EventItem[] newEvents = (await timepadClient.GetEvents(new[] {"id"}, new[] {452}, new[] {"Москва"}, checkLog.CheckTime)).Values;
                Log.Information("Got {Count} results", newEvents.Length);
                
                foreach (EventItem newEventItem in newEvents)
                {
                    EventItem eventItem = await timepadClient.GetEvent(newEventItem.Id);
                    if(eventItem.Location?.Address == null)
                        continue;
                    
                    string dateString = eventItem.startsAt.ToString("d MMMM, dddd", CultureInfo.CreateSpecificCulture("ru-RU"));
                    string mapsLink = "https://www.google.com/maps/search/".SetQueryParams(new
                    {
                        api=1,
                        query=eventItem.Location.Address
                    });
                    
                    string caption = $"*{eventItem.Name}*\n" +
                                     $"{eventItem.DescriptionShort}\n\n" +
                                     $"{eventItem.Url}\n" +
                                     $"*Проходит:* {dateString}.\n" +
                                     $"*Адрес:* `{eventItem.Location.Address}` ([на карте]({mapsLink}))";
                    
                    Log.Information("Sending {Id}", eventItem.Id);
                    await telegramClient.SendPhotoAsync(botConfig.ChatId, new InputOnlineFile(eventItem.PosterImage.DefaultUrl), caption, ParseMode.Markdown);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "An exception was occured:");
                return;
            }

            DateTime currentDateTime = DateTime.Now;
            Log.Information("Writing current DateTime: {DateTime}", currentDateTime);
            File.WriteAllText("checklog.json", JsonConvert.SerializeObject(new CheckLog {CheckTime = currentDateTime}));
        }
    }
}