using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotFootBall.Services
{
  public  interface  IStandingService
    {
        public  Task<Attachment> GetBotStading(string group);
        public Task<IEnumerable<dynamic>> GetJsonStading(string group);
    }
}
