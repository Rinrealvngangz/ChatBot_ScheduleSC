using BotFootBall.Models;
using System.Collections.Generic;


namespace BotFootBall.Services
{
  public  interface ISchedule
    {
        public void GetJsonSchedule();
        public List<MatchesModel> GetScheduleDay();
    }
}
