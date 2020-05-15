using System;

namespace Objects
{
    class RelevantMatchInfo
    {
        public DateTime finishTime {get; set;}
        public string summonerName {get; set;}
        public string championName {get; set;}
        public bool winner {get; set;}
        public long kills {get; set;}
        public long deaths {get; set;}
        public long assists {get; set;}
    }
}