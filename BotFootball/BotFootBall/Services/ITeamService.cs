using BotFootBall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotFootBall.Services
{
   public interface ITeamService
    {
        public Task<ScheduleModel> GetJsonTeamAsync(string team);
    }
}
