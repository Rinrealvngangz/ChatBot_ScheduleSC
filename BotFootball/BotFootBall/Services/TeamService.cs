using BotFootBall.Models;
using BotFootBall.Models.Team;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace BotFootBall.Services
{
    public  class TeamService : ITeamService
    {
        private readonly string key_secret = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Token")["key"];
        private static string vnTimeZoneKey = "SE Asia Standard Time";

        private static TimeZoneInfo vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(vnTimeZoneKey);

        private readonly string Location = "../BotFootBall/data/team.json";

        private readonly string LocationTeamById = "../BotFootBall/data/teamById.json";

        private readonly IStandingService _standingService;

       public TeamService(IStandingService standingService)
        {
            _standingService = standingService;
        }

        public async Task GetJsonToObjectAsync()
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage();
            httpClient.BaseAddress = new Uri("http://api.football-data.org/v2/competitions/2018/teams");
            httpClient.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Method = HttpMethod.Get;
            request.Headers.Add("X-Auth-Token", key_secret);

            HttpResponseMessage response = await httpClient.SendAsync(request);

            var responseString = await response.Content.ReadAsStringAsync();

            dynamic JsonObj = JsonConvert.DeserializeObject<ExpandoObject>(responseString, new ExpandoObjectConverter());
          
            var listTeam = new ListTeamModel();
            ((IEnumerable<dynamic>)JsonObj.teams).ToList().ForEach(x=>listTeam.ListTeam.Add(new TeamModel() { Id = Convert.ToString(x.id), Name =Convert.ToString(x.name) }) );
          
            if(listTeam.ListTeam.Count > 0) {
                string json = JsonConvert.SerializeObject(listTeam, Formatting.Indented);

                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    stream.Position = 0;
                    using (FileStream file = new FileStream(Location, FileMode.Create, FileAccess.Write))
                    {
                        stream.WriteTo(file);
                        file.Close();
                        file.Dispose();
                    }
                    stream.Close();
                    stream.Dispose();
                  
                }
            }
        
        }

        public async Task<ScheduleModel> GetJsonTeamAsync(string team)
        {
          
            string _group = string.Empty;
            string id = string.Empty;
            string formatName = FirstCharToUpper(team);
            using (StreamReader streamReader = new StreamReader(Location))
            {
                string json = streamReader.ReadToEnd();
           
                dynamic lsTeam = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                dynamic objTeam = ((IEnumerable<dynamic>)lsTeam.ListTeam).FirstOrDefault(x => x.Name == formatName);
               if(objTeam == null)
                {
                    return null;
                }
                 id =   Convert.ToString(objTeam.Id);
                streamReader.Close();
                streamReader.Dispose();
            }

                HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage();
            httpClient.BaseAddress = new Uri("http://api.football-data.org/v2/teams/"+ id + "/matches");
            httpClient.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Method = HttpMethod.Get;
            request.Headers.Add("X-Auth-Token", key_secret);

            HttpResponseMessage response = await httpClient.SendAsync(request);

            var responseString = await response.Content.ReadAsStringAsync();

            dynamic JsonObj = JsonConvert.DeserializeObject<ExpandoObject>(responseString, new ExpandoObjectConverter());

            var matchesModel = new ScheduleModel();
            matchesModel.ScheduleMatch = new List<MatchesModel>();

            var listTeam = new ScheduleModel();
            listTeam.ScheduleMatch = new List<MatchesModel>();

            foreach (var item in ((IEnumerable<dynamic>)JsonObj.matches).ToList())
            {
                if (string.IsNullOrEmpty(_group))
                {
                    _group = Convert.ToString(item.group);
                }

                var fulltime = new FullTimeModel();
                if (item.score.fullTime.homeTeam != null && item.score.fullTime.awayTeam != null)
                {

                    fulltime.HomeTeam = Convert.ToInt32(item.score.fullTime.homeTeam);
                    fulltime.AwayTeam = Convert.ToInt32(item.score.fullTime.awayTeam);
                }

                var matchModel = new MatchesModel()
                {
                    Group = Convert.ToString(item.group),
                    Status = Convert.ToString(item.status),
                    UctDate = FormatTimeZoneVietNam(Convert.ToDateTime(item.utcDate)),
                    FullTime = fulltime,
                    AwayTeam = Convert.ToString(item.homeTeam.name),
                    HomeTeam = Convert.ToString(item.awayTeam.name),
                };
                listTeam.ScheduleMatch.Add(matchModel);
            }

            dynamic group = await _standingService.GetJsonStading(_group.Replace(" ", "_").ToUpper());

            foreach( var teams in ((IEnumerable<dynamic>)group).ToList())
            {
                foreach(var item in teams.table)
                {
                     if(Convert.ToString(item.team.id) == id)
                    {
                        var teamModel = new TeamModel()
                        {
                            Id = Convert.ToString(id),
                            Position = Convert.ToString(item.position),
                            Name = Convert.ToString(item.team.name),
                            CrestUrl = Convert.ToString(item.team.crestUrl),
                            Won = Convert.ToString(item.won),
                            Draw = Convert.ToString(item.draw),
                            Lost = Convert.ToString(item.lost),
                            Points = Convert.ToString(item.points),
                        };
                        listTeam.teamModel = teamModel;
                        break;
                    }    
                }
                 
            }
    
            return listTeam;
        }

        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        private DateTime FormatTimeZoneVietNam(DateTime dateTime)
        {

            DateTime ngaygiohientai = TimeZoneInfo.ConvertTimeFromUtc(dateTime, vnTimeZone);
            return ngaygiohientai;
        }
    }

}
