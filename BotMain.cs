using System;
using System.Timers;
using System.Threading.Tasks;
using ApiClients;

namespace YetAnotherStupidDiscordBot
{
    public class BotMain
    {
        public static void Main(string[] args) 
        {
            // Timer that will run the match history check every 30 seconds.
            var checkMatchHistoryTimer = new System.Timers.Timer(30000);
            var riot = new RiotApiClient();
            var discord = new DiscordApiClient(riot);

            // Register Ctrl-C and process exit handlers
            //AppDomain.CurrentDomain.ProcessExit += new EventHandler((object sender, EventArgs args) => {discord.programExitHandler();});
            Console.CancelKeyPress += new ConsoleCancelEventHandler((object sender, ConsoleCancelEventArgs args) => {discord.programExitHandler();});

            // Register the match history check to its timer
            checkMatchHistoryTimer.Elapsed += riot.checkMatchHistories;
            checkMatchHistoryTimer.Enabled = true;
            
            // Log in the discord client.
            discord.DiscordInitAsync().GetAwaiter().GetResult();
        }
    }
}