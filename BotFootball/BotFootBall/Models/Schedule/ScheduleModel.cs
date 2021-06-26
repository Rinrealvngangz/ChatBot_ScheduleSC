using BotFootBall.Models.Team;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotFootBall.Models
{
    public class ScheduleModel
    {
        public List<MatchesModel> ScheduleMatch { get; set; }

        public TeamModel teamModel { get; set; }
    }
}
