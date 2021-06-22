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

namespace BotFootBall.Services
{
    public class ScheduleService : ISchedule
    {
      private readonly string  key_secret = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Token")["key"];

        private readonly string Location = "../BotFootBall/data/schedule.json";
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
            if(new FileInfo(Location).Length == 0)
            {
                HttpResponseMessage response = await httpClient.SendAsync(request);

                var responseString = await response.Content.ReadAsStringAsync();


                var statusCode = response.StatusCode;

                if (statusCode.ToString().Equals("OK"))
                {
                    MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(responseString));
                    stream.Position = 0;
                    using (FileStream file = new FileStream(Location, FileMode.Create, FileAccess.Write))
                    {
                        stream.WriteTo(file);
                        file.Close();
                        stream.Close();
                        file.Dispose();
                    }

                 
                    //   var JSONresult = JsonConvert.SerializeObject( stream , Formatting.Indented);
                    //  File.WriteAllText(@"D:\euro2021.json", JSONresult);

                }
            }
        
        }

        public List<MatchesModel> GetScheduleDay()
        {
            ScheduleModel scheduleModel = new ScheduleModel();
            scheduleModel.ScheduleMatch = new List<MatchesModel>();
            using (StreamReader streamReader = new StreamReader(Location))
            {
                string json = streamReader.ReadToEnd();
                var  schedule =  JObject.Parse(json)["matches"];
                int dem = 0;
                foreach(var item in schedule)
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
                    matchesModel.UctDate = item.Value<DateTime>("utcDate");
                    matchesModel.HomeTeam = item["homeTeam"].Value<string>("name");
                    matchesModel.AwayTeam = item["awayTeam"].Value<string>("name");
                    scheduleModel.ScheduleMatch.Add(matchesModel);
                    dem++;
                    if(dem > 2)
                    {
                        break;
                    }
                }

            }
            return scheduleModel.ScheduleMatch;
        }
    }
}
