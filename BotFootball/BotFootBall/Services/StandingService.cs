using BotFootBall.Models.Stading;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BotFootBall.Services
{
    public class StandingService : IStandingService
    {
        private readonly string key_secret = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Token")["key"];

        private readonly string Location = "../BotFootBall/data/standing.json";



        public async Task<IEnumerable<dynamic>> GetJsonStading(string group)
        {
            string json = string.Empty;
        
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage();
            httpClient.BaseAddress = new Uri("http://api.football-data.org/v2/competitions/2018/standings");
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
                    using (FileStream file = new FileStream(Location, FileMode.Create, FileAccess.Write))
                    {
                        stream.WriteTo(file);
                        file.Close();
                        file.Dispose();
                    }
                    stream.Close();
                    stream.Dispose();

                }
                
                using (StreamReader streamReader = new StreamReader(Location))
                {
                    json = await streamReader.ReadToEndAsync();
                    streamReader.Close();
                    streamReader.Dispose();
                }
            
            }
            dynamic stading = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            IEnumerable<dynamic> groups = ((IEnumerable<dynamic>)stading.standings).Where(t => t.group == group);

            return groups;
        }
       
        private async Task<StandingModel> ReadStadingFromJsonToObj(string group)
        {
           
                var groups= await GetJsonStading(group);
            var stadingsObj = new StandingModel();
            foreach (var gr in groups)
                    {

                        foreach (var item in gr.table)
                        {
                            var name = item.team.name;
                            var iconURL = item.team.crestUrl;
                            var point = item.points + " đ";
                            var team = new Team()
                            {
                                Name = name,
                                Point = Convert.ToString(point),
                                CrestUrl = iconURL
                            };
                            stadingsObj.Teams.Add(team);

                        }
                    }
                 
            return stadingsObj;
        }
        public async Task<Attachment> GetBotStading(string group)
        {
            var stadings = await ReadStadingFromJsonToObj(group);
            var listReceipItem = new List<ReceiptItem>();
           

            foreach (var item in stadings.Teams)
            {
                var Recei = new ReceiptItem()
                {
                    Text = item.Name,
                    Image = new CardImage(item.CrestUrl),
                    Price = item.Point

                };
                listReceipItem.Add(Recei);
            }
            var attac = new ReceiptCard()
            {
                Title = group,
                Items = listReceipItem
            };
            return attac.ToAttachment();

        }
    }
}
