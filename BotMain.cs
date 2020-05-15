using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Objects;

namespace YetAnotherStupidDiscordBot
{
    class BotMain
    {
        private DiscordSocketClient discordClient;
        private RiotApiClient riotClient;
        private string lossAnnounceFmt = "@here Summoner {0} has just lost a game as {1}!\nHis k/d/a was {2}/{3}/{4}.\nWhat a fucking loser!";
        private RelevantMatchInfo lastMatchChecked;

        public BotMain()
        {
            this.discordClient = new DiscordSocketClient();
            this.riotClient = new RiotApiClient();
            this.lastMatchChecked = new RelevantMatchInfo();
        }

        public async Task DiscordInitAsync()
	    {
            this.discordClient.Log += Log;

            await this.discordClient.LoginAsync(TokenType.Bot, "NjEzNTc5ODMzMzgwNzAwMTkx.Xrnejg.QtKtOZkIDnifPVDTQM6_0D0w3tQ");
            await this.discordClient.StartAsync();

            this.discordClient.MessageReceived += MessageReceived;

            // Tell the bot to listen for a gameFinished event.  Riot client have already been initialized in the bot before this call
            this.riotClient.gameFinished += gameFinished;

            // Block this task until the program is closed.
            await Task.Delay(-1);
	    }

        private int gameFinished(RelevantMatchInfo lastMatchInfo)
        {
            // Jalen User ID: 279845556166197251
            // Sucks At League of Legends Role ID: 709492755386204225
            // Server ID: 676302856432779264
            SocketGuild server = this.discordClient.GetGuild(676302856432779264);
            SocketGuildUser user = server.GetUser(279845556166197251);
            SocketRole shitterLandRole = server.GetRole(709492755386204225);

            // If the last match has already been detected recently don't do anything
            if (lastMatchInfo.finishTime.Equals(this.lastMatchChecked.finishTime)) {
                return 1;
            } else if (!lastMatchInfo.winner) {
                // If game lost, announce the loss and assign the punishment role
                SocketTextChannel channel = this.discordClient.GetChannel(691481574335840368) as SocketTextChannel;
                string msg = String.Format(this.lossAnnounceFmt, lastMatchInfo.summonerName, lastMatchInfo.championName, lastMatchInfo.kills, lastMatchInfo.deaths, lastMatchInfo.assists);
                channel.SendMessageAsync(msg, true);
                user.AddRoleAsync(shitterLandRole);
            } else if (lastMatchInfo.winner) {
                // If game won, remove the punishment role
                user.RemoveRoleAsync(shitterLandRole);
            }

            return 0;
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

            SocketGuild server = this.discordClient.GetGuild(676302856432779264);
            SocketGuildUser user = server.GetUser(279845556166197251);
            SocketRole shitterLandRole = server.GetRole(709492755386204225);

            // jalen_zone channel id = 691481574335840368
            if (message.Content == "!ping" && message.Channel.Id == 691481574335840368) {
                await message.Channel.SendMessageAsync("Pong!");
            } else if (Regex.Match(message.Content, "^.*big chungus.*$", RegexOptions.IgnoreCase).Success) {
                await this.JoinVoiceJustToPlayBigChungus();
            } else if (message.Content.Equals("-shitterland")) {
                await user.AddRoleAsync(shitterLandRole);
            } else if (message.Content.Equals("-unshitterland")) {
                await user.RemoveRoleAsync(shitterLandRole);
            }
        }

        [Command("join", RunMode = RunMode.Async)] 
        private async Task JoinVoiceJustToPlayBigChungus() {
            // pizza_bois voice channel id = 676302856432779303
            // music_bot channel id = 693200354376024254
            var voiceChannel = this.discordClient.GetChannel(676302856432779303) as SocketVoiceChannel;
            var audioClient = await voiceChannel.ConnectAsync(true, true);
            var musicChannel = this.discordClient.GetChannel(693200354376024254) as SocketTextChannel;
            await musicChannel.SendMessageAsync("-play big chungus 2");
        }

        public static void Main(string[] args) 
        {
            var bot = new BotMain();

            // Start the match history check loop
            Task.Run(() => bot.riotClient.matchHistoryCheckLoop());

            bot.DiscordInitAsync().GetAwaiter().GetResult();
        }
    }
}
