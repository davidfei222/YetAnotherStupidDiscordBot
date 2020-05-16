using System.Collections.Generic;

namespace Objects
{
    /**
    This class keeps a table of LoL summoner names mapped to their Discord IDs.
    **/
    class SummonerDiscordIdMappings
    {
        public static readonly Dictionary<string, ulong> mappings = new Dictionary<string, ulong>()
        {
            // Jalen
            {"AnimePenguin", 279845556166197251}
        }; 
    }
}