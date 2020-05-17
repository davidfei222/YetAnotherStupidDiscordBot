using System.Threading.Tasks;
using ApiClients;

namespace YetAnotherStupidDiscordBot
{
    public class BotMain
    {
        public static void Main(string[] args) 
        {
            var bot = new DiscordApiClient();

            // Start the match history check loop in a separate thread
            bot.runMatchHistoryCheckLoop();
            
            // Log in the discord client.
            bot.DiscordInitAsync().GetAwaiter().GetResult();
        }
    }
}