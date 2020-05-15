using System.Threading.Tasks;
using ApiClients;

namespace YetAnotherStupidDiscordBot
{
    class BotMain
    {
        private DiscordApiClient discordClient;
        private RiotApiClient riotClient;

        public BotMain()
        {
            this.discordClient = new DiscordApiClient();
            this.riotClient = new RiotApiClient();
        }

        public static void Main(string[] args) 
        {
            var bot = new BotMain();

            // Start the match history check loop in a separate thread
            Task.Run(() => bot.riotClient.matchHistoryCheckLoop());
            // Register discord-related event handler for gameFinished.  Riot client should have already been initialized in the bot before this call
            bot.riotClient.gameFinished += bot.discordClient.gameFinished;

            // Log in the discord client.
            bot.discordClient.DiscordInitAsync().GetAwaiter().GetResult();
        }
    }
}