using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;
using BotFootBall.Models;
using Newtonsoft.Json;
using Microsoft.Bot.Builder;
using System.Threading;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace BotFootBall.Services
{
    public class ScheduleService : ISchedule
    {
      private readonly string  key_secret = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Token")["key"];

        private static string vnTimeZoneKey = "SE Asia Standard Time";
        private static TimeZoneInfo vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(vnTimeZoneKey);
        private ThumbnailCard thumnailCard;
     
        private List<ThumbnailCard> listThumnailCards;
        private List<string> stringResultSchedule;
        private readonly string Location = "../BotFootBall/data/schedule.json";
        private readonly TimersManage _timersManage;
        public ScheduleService() 
        {
         
        }
        public async void GetJsonSchedule()
        {

            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage();
            httpClient.BaseAddress = new Uri("http://api.football-data.org/v2/competitions/2018/matches");
            httpClient.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Method = HttpMethod.Get;
            request.Headers.Add("X-Auth-Token", key_secret);

            HttpResponseMessage response = await httpClient.SendAsync(request);

            var responseString = await response.Content.ReadAsStringAsync();


            var statusCode = response.StatusCode;

            if (statusCode.ToString().Equals("OK"))
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(responseString)))
                {
                    stream.Position = 0;
                    using (FileStream file = new FileStream("../BotFootBall/data/schedule.json", FileMode.Create, FileAccess.Write))
                    {
                        stream.WriteTo(file);
                        file.Close();
                        file.Dispose();
                    }
                    stream.Close();
                    stream.Dispose();
                    //   var JSONresult = JsonConvert.SerializeObject( stream , Formatting.Indented);
                    //  File.WriteAllText(@"D:\euro2021.json", JSONresult);
                }

            }
        }
        public List<MatchesModel> GetScheduleDay(DateTime dateTime)
        {
             GetJsonSchedule();
            ScheduleModel scheduleModel = new ScheduleModel();
            scheduleModel.ScheduleMatch = new List<MatchesModel>();
            using (StreamReader streamReader = new StreamReader(Location))
            {
                string json = streamReader.ReadToEnd();
                var  schedule =  JObject.Parse(json)["matches"];
                foreach(var item in schedule.Where(x=> FormatDate(x.Value<DateTime>("utcDate")) == FormatDate(dateTime)))
                {
                    MatchesModel matchesModel = new MatchesModel();
                  
                    var scoreObj = item["score"];
                    var fulltimeObj = JObject.Parse(scoreObj["fullTime"].ToString());
                    if (fulltimeObj["awayTeam"].Type != JTokenType.Null && fulltimeObj["awayTeam"].Type != JTokenType.Null)
                    {
                        FullTimeModel fullTimeModel = new FullTimeModel();
                       fullTimeModel.AwayTeam = fulltimeObj.Value<int>("awayTeam");
                        fullTimeModel.HomeTeam = fulltimeObj.Value<int>("homeTeam");
                        matchesModel.FullTime = fullTimeModel;
                    }
                    matchesModel.Group = item.Value<string>("group");
                    matchesModel.UctDate = FormatTimeZoneVietNam(item.Value<DateTime>("utcDate"));
                    matchesModel.HomeTeam = item["homeTeam"].Value<string>("name");
                    matchesModel.AwayTeam = item["awayTeam"].Value<string>("name");
                    scheduleModel.ScheduleMatch.Add(matchesModel);
                  
                }
                streamReader.Close();
                streamReader.Dispose();
            }
            return scheduleModel.ScheduleMatch;
        }
        public async void DisPlayScheduleByStep(DateTime currentdateTime,WaterfallStepContext stepContext , CancellationToken cancellation )
        {
            DisPlaySchedule(currentdateTime);
            if (stringResultSchedule.Count > 0)
            {
                stringResultSchedule.ForEach(async x => await stepContext.Context.SendActivityAsync(MessageFactory.Text(x), cancellation));
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Không có trận đấu nào hết"), cancellation);
            }
        }

        private string StringDisplaySchedule(dynamic item)
        {

            var hasFullTime = string.Empty;
            var team = string.Empty;
            if (item.FullTime != null)
            {
                hasFullTime = $"{item.FullTime.HomeTeam} - {item.FullTime.AwayTeam}";
            }
            var group = $"{item.Group}\n\n";
            if (!hasFullTime.Equals(string.Empty))
            {
                team = string.Concat($"(Home) {item.HomeTeam}  {hasFullTime} ", $" {item.AwayTeam} (Away) \n\n");
            }
            else
            {
                team = string.Concat($"(Home) {item.HomeTeam} vs", $" {item.AwayTeam}  (Away)\n\n");
            }

            var timeMatch = string.Concat(team, $"({item.UctDate})");

            var result = string.Concat(group, timeMatch);
            return result;
        }
        private  void DisPlaySchedule(DateTime currentDateTime)
        {

            List<MatchesModel> Listmatches = GetScheduleDay(currentDateTime);
            stringResultSchedule = new List<string>();
            foreach (var item in Listmatches)
            {
                stringResultSchedule.Add(StringDisplaySchedule(item));

            }
        }
        private DateTime FormatTimeZoneVietNam(DateTime dateTime)
        {
         
            DateTime ngaygiohientai = TimeZoneInfo.ConvertTimeFromUtc(dateTime, vnTimeZone);
            return ngaygiohientai;
        }
        private DateTime FormatDate(DateTime dateTime)
        {
         
            DateTime ngayhientai = DateTime.Parse(TimeZoneInfo.ConvertTimeFromUtc(dateTime, vnTimeZone).ToShortDateString());
            return ngayhientai;
        }

        public List<DateTime> GetDateTimeOfWeeks()
        {
            DateTime today = DateTime.UtcNow;
            int currentDayOfWeek = (int)today.DayOfWeek;
            DateTime sunday = today.AddDays(-currentDayOfWeek);
            DateTime monday = sunday.AddDays(1);
       
            if (currentDayOfWeek == 0)
            {
                monday = monday.AddDays(-7);
            }
            var dates = Enumerable.Range(0, 7).Select(days => monday.AddDays(days)).ToList();
            return dates;
        }

        public async Task<DialogTurnResult> AppointmentSchedule(DateTime currentdateTime, WaterfallStepContext stepContext, CancellationToken cancellation)
        {
            DisPlayThumnail(currentdateTime);
            if (listThumnailCards.Count > 0)
            {
                List<Attachment> attachments = new List<Attachment>();
                foreach (var item in listThumnailCards)
                {
                    attachments.Add(item.ToAttachment());
                }
                var promOption = new PromptOptions()
                {
                    Prompt = new Activity
                    {
                        Type = ActivityTypes.Message,
                        Attachments = attachments
                    }
                };

                return await stepContext.PromptAsync("appointmentSchedule", promOption, cancellation);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Không có trận đấu nào hết"), cancellation);
            }
            return await stepContext.NextAsync(null, cancellation);
        }
        private ThumbnailCard DisplayThumnailCardSchedule(dynamic item)
        {
         
            var group = $"{item.Group}\n\n";       
               string team = string.Concat($"(Home) {item.HomeTeam} vs", $" {item.AwayTeam}  (Away)\n\n");
                var timeMatch = string.Concat(team, $"({item.UctDate})");
                //  var result = string.Concat(group, timeMatch);
                thumnailCard = new ThumbnailCard()
                {
                    Title = group,
                    Text = timeMatch,
                    Buttons = new List<CardAction>() { new CardAction() { Title = "Đặt lịch hẹn ", Type = ActionTypes.PostBack, Value = $"{ item.UctDate }" } }

                };
             
            return thumnailCard;
        }
        private void DisPlayThumnail(DateTime currentDateTime)
        {

            List<MatchesModel> Listmatches = GetScheduleDay(currentDateTime);
            listThumnailCards = new List<ThumbnailCard>();
            foreach (var item in Listmatches)
            {
                if (item.FullTime == null)
                {
                    listThumnailCards.Add(DisplayThumnailCardSchedule(item));
                }
                  

            }

        }

    }
}
