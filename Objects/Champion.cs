using System.Collections.Generic;

namespace Objects
{
    class Champion
    {
        public string version {get; set;}
        public string id {get; set;}
        public string key {get; set;}
        public string name {get; set;}
        public string title {get; set;}
        public string blurb {get; set;}
        public ChampionInfo info {get; set;}
        public ChampionImage image {get; set;}
        public List<string> tags {get; set;}
        public string partype {get; set;}
        public ChampionStats stats {get; set;}

        public Champion() {}
    }
}