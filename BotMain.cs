using System.Threading.Tasks;
using ApiClients;

namespace YetAnotherStupidDiscordBot
{
    class BotMain
    {
        private DiscordApiClient discordClient;

        public BotMain()
        {
            // Server ID: 676302856432779264
            this.discordClient = new DiscordApiClient(676302856432779264);
        }

        public static void Main(string[] args) 
        {
            var bot = new BotMain();

            // Start the match history check loop in a separate thread
            bot.discordClient.runMatchHistoryCheckLoop();
            
            // Log in the discord client.
            bot.discordClient.DiscordInitAsync().GetAwaiter().GetResult();
        }
    }
}