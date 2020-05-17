using System.Collections.Generic;

namespace Objects
{
    // This class keeps a bunch of static data.
    public class StaticData
    {
        public static ulong serverId = 676302856432779264; // Quarantine Bois server
        public static ulong punishmentRoleId = 709492755386204225; // Sucks At League of Legends role
        public static ulong jalenZoneId = 691481574335840368; // jalen_zone channel
        public static ulong pizzaBoisId = 676302856432779303; // pizza_bois voice channel
        // Table mapping LoL summoner names to Discord IDs
        public static readonly Dictionary<string, ulong> summonerToDiscordMappings = new Dictionary<string, ulong>()
        {
            // Jalen
            {"AnimePenguin", 279845556166197251}
        }; 
    }
}