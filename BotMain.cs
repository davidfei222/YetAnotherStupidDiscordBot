using System.Threading.Tasks;
using ApiClients;

namespace YetAnotherStupidDiscordBot
{
    public class BotMain
    {
        public static void Main(string[] args) 
        {
            var riot = new RiotApiClient();
            var discord = new DiscordApiClient(riot);

            // Start the match history check loop in a separate thread
            Task.Run(riot.matchHistoryCheckLoop);
            
            // Log in the discord client.
            discord.DiscordInitAsync().GetAwaiter().GetResult();
        }
    }
}