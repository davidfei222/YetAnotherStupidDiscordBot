using System;

namespace Objects
{
    public class RelevantMatchInfo
    {
        public string accountId {get; set;}
        public DateTime finishTime {get; set;}
        public string summonerName {get; set;}
        public string championName {get; set;}
        public bool winner {get; set;}
        public long kills {get; set;}
        public long deaths {get; set;}
        public long assists {get; set;}
    }
}