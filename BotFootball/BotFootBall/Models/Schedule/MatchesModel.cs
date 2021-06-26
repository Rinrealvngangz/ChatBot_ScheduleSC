using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotFootBall.Models
{
    public class MatchesModel
    {
        public DateTime UctDate { get; set; }
        public string Group { get; set; }

        public string AwayTeam { get; set; }

        public string HomeTeam { get; set; }

        public string Status { get; set; }

        public FullTimeModel FullTime { get; set; }


    }
}
