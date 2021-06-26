using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BotFootBall.Services
{
    public class TimersManage 
    {
        private readonly IAdapterIntegration _adapter;
       
        public TimersManage(IAdapterIntegration adapter)
        {
            _adapter = adapter;
        }

        public List<Timer> List { get; set; } = new List<Timer>();

        public void AddTimer(IHttpClientFactory factory ,ConversationReference reference, int seconds)
        {
            var timer = new Timer(factory ,_adapter, reference, seconds);

            Task.Run(() => timer.Start());

            List.Add(timer);
        }
    }
}
