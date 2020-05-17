using System.Collections.Generic;

namespace Objects
{
    public class ChampionData
    {
        public string type {get; set;}
        public string format {get; set;}
        public string version {get; set;}
        public Dictionary<string, Champion> data {get; set;}

        public ChampionData() {}
    }
}