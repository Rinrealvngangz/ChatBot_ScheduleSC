using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotFootBall.Models.Stading
{
    public class StandingModel
    {
        public string Group { get; set; }

        public  List<Team> Teams { get; set; } = new List<Team>();
    }
}
