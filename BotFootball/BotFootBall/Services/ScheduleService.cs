using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;


namespace BotFootBall.Services
{
    public class ScheduleService : ISchedule
    {
      private readonly string  key_secret = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Token")["key"];
     
        public async void GetSchedule()
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
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(responseString));
                stream.Position = 0;
                using (FileStream file = new FileStream("../BotFootBall/data/schedule.json", FileMode.Create, FileAccess.Write))
                {
                    stream.WriteTo(file);
                    file.Close();
                    stream.Close();
                }

                //   var JSONresult = JsonConvert.SerializeObject( stream , Formatting.Indented);
                //  File.WriteAllText(@"D:\euro2021.json", JSONresult);
           
            }
        }
    }
}
