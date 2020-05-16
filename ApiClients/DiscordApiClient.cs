using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Objects;

namespace ApiClients
{
    class DiscordApiClient
    {
        public DiscordSocketClient discordSocketClient;
        private string lossAnnounceFmt = "@here Summoner {0} has just lost a game as {1}!\nHis k/d/a was {2}/{3}/{4}.\nWhat a fucking loser!";
        private RelevantMatchInfo lastMatchChecked;
        private RiotApiClient riotApiClient;
        private ulong serverId = 676302856432779264; // Quarantine Bois server
        private ulong punishmentRoleId = 709492755386204225; // Sucks At League of Legends role
        private ulong jalenZoneId = 691481574335840368; // jalen_zone channel
        private ulong pizzaBoisId = 676302856432779303; // pizza_bois voice channel

        public DiscordApiClient()
        {
            this.discordSocketClient = new DiscordSocketClient();
            this.riotApiClient = new RiotApiClient();
            // Start with a blank last match checked
            this.lastMatchChecked = new RelevantMatchInfo();
        }

        public void runMatchHistoryCheckLoop()
        {
            Task.Run(this.riotApiClient.matchHistoryCheckLoop);
        }

        public async Task DiscordInitAsync()
        {
            this.discordSocketClient.Log += Log;

            await this.discordSocketClient.LoginAsync(TokenType.Bot, "NjEzNTc5ODMzMzgwNzAwMTkx.Xrnejg.QtKtOZkIDnifPVDTQM6_0D0w3tQ");
            await this.discordSocketClient.StartAsync();

            // Register Discord API event handlers
            this.discordSocketClient.MessageReceived += MessageReceived;
            this.discordSocketClient.Ready += Ready;

            // Register Riot client event handlers
            this.riotApiClient.gameFinished += this.gameFinished;

            // Let the bot run infinitely
            await Task.Delay(-1);
        }

        private Task Ready()
        {
            // Let riotApiClient know that discord is ready for its events
            this.riotApiClient.signalDiscordReady();
            return Task.CompletedTask;
        }

        private int gameFinished(RelevantMatchInfo lastMatchInfo)
        {
            Console.WriteLine("A recently finished game was detected for " + lastMatchInfo.summonerName + ".");

            // If the last match has already been detected recently don't do anything
            if (lastMatchInfo.finishTime.Equals(this.lastMatchChecked.finishTime)) {
                Console.WriteLine("The game was already checked.");
                return 1;
            } else if (!lastMatchInfo.winner) {
                Console.WriteLine("The game was lost!");
                // If game lost, announce the loss and assign the punishment role
                SocketTextChannel channel = this.discordSocketClient.GetChannel(this.jalenZoneId) as SocketTextChannel;
                string msg = String.Format(this.lossAnnounceFmt, lastMatchInfo.summonerName, lastMatchInfo.championName, lastMatchInfo.kills, lastMatchInfo.deaths, lastMatchInfo.assists);
                channel.SendMessageAsync(msg, true);
                this.addOrRemoveRole(SummonerDiscordIdMappings.mappings[lastMatchInfo.summonerName], this.punishmentRoleId, true);
            } else if (lastMatchInfo.winner) {
                Console.WriteLine("The game was won!");
                // If game won, remove the punishment role
                this.addOrRemoveRole(SummonerDiscordIdMappings.mappings[lastMatchInfo.summonerName], this.punishmentRoleId, false);
            }

            this.lastMatchChecked = lastMatchInfo;

            return 0;
        }

        private void addOrRemoveRole(ulong userId, ulong roleId, bool add)
        {
            SocketGuild server = this.discordSocketClient.GetGuild(this.serverId);
            SocketGuildUser user = server.GetUser(userId);
            SocketRole role = server.GetRole(roleId);

            if (add) {
                user.AddRoleAsync(role);
            } else {
                user.RemoveRoleAsync(role);
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

            // jalen_zone channel id = 691481574335840368
            if (message.Content == "!ping" && message.Channel.Id == this.jalenZoneId) {
                await message.Channel.SendMessageAsync("Pong!");
            } else if (message.Content.Equals("-shitterland")) {
                this.addOrRemoveRole(SummonerDiscordIdMappings.mappings["AnimePenguin"], this.punishmentRoleId, true);
            } else if (message.Content.Equals("-unshitterland")) {
                this.addOrRemoveRole(SummonerDiscordIdMappings.mappings["AnimePenguin"], this.punishmentRoleId, false);
            }
        }

        [Command("join", RunMode = RunMode.Async)]
        private async Task JoinVoiceJustToPlayBigChungus()
        {
            var voiceChannel = this.discordSocketClient.GetChannel(this.pizzaBoisId) as SocketVoiceChannel;
            var audioClient = await voiceChannel.ConnectAsync(true, true);
        }
    }
}
