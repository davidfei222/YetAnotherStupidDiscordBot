using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace YetAnotherStupidDiscordBot
{
    class BotMain
    {
        private DiscordSocketClient discordClient;
        private RiotApiClient riotClient;
        private List<string> monitoredSummoners;

        public static void Main(string[] args) =>
            new BotMain().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
	    {
            this.monitoredSummoners = new List<string>()
            {
                "AnimePenguin"
            };

            this.riotClient = new RiotApiClient();
            this.discordClient = new DiscordSocketClient();

            this.discordClient.Log += Log;

            await this.discordClient.LoginAsync(TokenType.Bot, "NjEzNTc5ODMzMzgwNzAwMTkx.Xrnejg.QtKtOZkIDnifPVDTQM6_0D0w3tQ");
            await this.discordClient.StartAsync();

            this.discordClient.MessageReceived += MessageReceived;

            // Begin the loop of checking the Riot API for last match updates for each monitored player
            // Running it this way will warn about not using await on this call but that's fine because I want this thread to run infinitely 
            // in the background which will block this method forever if an await is used.
            #pragma warning disable 4014
            Task.Run(matchHistoryCheckLoop);
            await this.Log(new LogMessage(LogSeverity.Info, "BotMain", "Match history loop thread started"));

            // Block this task until the program is closed.
            await Task.Delay(-1);
	    }

        private void matchHistoryCheckLoop()
        {
            while (true) {
                Console.WriteLine("checking last match state for monitored summoners");

                foreach (string summoner in this.monitoredSummoners) {
                    this.checkSummonerMatchHistory(summoner);
                }

                Thread.Sleep(30000);
            }
        }

        private void checkSummonerMatchHistory(string summoner)
        {
            var lastMatchInfo = this.riotClient.checkLastMatchWin(summoner);

            if (lastMatchInfo != null) {
                // Jalen User ID: 279845556166197251
                // Jalen Purgatory Role ID: 709492755386204225
                // Server ID: 676302856432779264
                Console.WriteLine("Announced state: " + this.riotClient.hasAnnounced);
                SocketGuildUser user = this.discordClient.GetUser(279845556166197251) as SocketGuildUser;
                SocketRole shitterLandRole = this.discordClient.GetGuild(676302856432779264).GetRole(709492755386204225);

                if(!lastMatchInfo.winner && !this.riotClient.hasAnnounced) {
                    // jalen_zone id = 691481574335840368
                    Console.WriteLine(summoner + " lost the game recently!");
                    SocketTextChannel channel = this.discordClient.GetChannel(691481574335840368) as SocketTextChannel;
                    string msg = "@here Summoner " + summoner + " has just lost a game as " + lastMatchInfo.championName + 
                        "!\nHis k/d/a was " + lastMatchInfo.kills + "/" + lastMatchInfo.deaths + "/" + lastMatchInfo.assists + 
                        ".\nWhat a fucking loser!";
                    channel.SendMessageAsync(msg, true);
                    
                    user.AddRoleAsync(shitterLandRole);
                    // Mark announcement happening so it doesn't keep saying it during the next run of the loop
                    this.riotClient.hasAnnounced = true;
                } else if (lastMatchInfo.winner) {
                    // If he won his last game, remove the punishment role
                    user.RemoveRoleAsync(shitterLandRole);
                }
            } else {
                Console.WriteLine(summoner + " has not played a game recently enough to warrant a message.");
            }
        }

        private Task Log(LogMessage msg)
        {
	        Console.WriteLine(msg.ToString());
	        return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            await this.Log(new LogMessage(LogSeverity.Info, message.Author.Username, "message received in " + message.Channel.Name));
            // Prevent infinite message loops by ignoring everything that bots say (including itself)
            if (message.Author.IsBot) {
                return;
            }

            if (message.Content == "!ping" && message.Channel.Name.Equals("jalen_zone")) {
                await message.Channel.SendMessageAsync("Pong!");
            } else if (Regex.Match(message.Content, "^.*big chungus.*$", RegexOptions.IgnoreCase).Success) {
                // music_bot channel id = 693200354376024254
                var musicChannel = this.discordClient.GetChannel(693200354376024254) as SocketTextChannel;
                await this.JoinVoiceJustToPlayBigChungus();
                await musicChannel.SendMessageAsync("-play big chungus 2");
            }
        }

        private async Task JoinVoiceJustToPlayBigChungus() {
            // pizza_bois voice channel id = 676302856432779303
            var voiceChannel = this.discordClient.GetChannel(676302856432779303) as SocketVoiceChannel;
            var audioClient = await voiceChannel.ConnectAsync(true, true);
        }
    }
}
