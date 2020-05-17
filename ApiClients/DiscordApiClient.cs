using System;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Objects;

namespace ApiClients
{
    public class DiscordApiClient
    {
        private DiscordSocketClient discordSocketClient;
        private CommandService commandService;
        private string lossAnnounceFmt = "@here Summoner {0} has just lost a game as {1}!\nHis k/d/a was {2}/{3}/{4}.\nWhat a fucking loser!";
        private RelevantMatchInfo lastMatchChecked;
        private RiotApiClient riotApiClient;

        public DiscordApiClient()
        {
            this.discordSocketClient = new DiscordSocketClient();
            this.commandService = new CommandService();
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

            await this.discordSocketClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken"));
            await this.discordSocketClient.StartAsync();

            // Register Discord event handlers
            this.discordSocketClient.MessageReceived += HandleCommandAsync;
            this.discordSocketClient.Ready += Ready;

            // Register Discord commands
            await this.commandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);

            // Let the bot run infinitely
            await Task.Delay(-1);
        }

        private Task Ready()
        {
            // Register Riot client event handlers once Discord is ready.
            // Clear all existing event handlers before adding this on to prevent multiple copies of the event handler being registered.
            // (If Discord forces a reconnect for whatever reason this tends to happen)
            this.riotApiClient.gameFinished -= this.gameFinishedHandler;
            this.riotApiClient.gameFinished += this.gameFinishedHandler;
            this.Log(new LogMessage(LogSeverity.Info, "Gateway", "gameFinished event handler registered"));
            return Task.CompletedTask;
        }

        private int gameFinishedHandler(RelevantMatchInfo lastMatchInfo)
        {
            Console.WriteLine("A recently finished game was detected for " + lastMatchInfo.summonerName + ".");

            // If the last match has already been detected recently don't do anything
            if (lastMatchInfo.finishTime.Equals(this.lastMatchChecked.finishTime)) {
                Console.WriteLine("The game was already checked.");
                return 1;
            } else if (!lastMatchInfo.winner) {
                Console.WriteLine("The game was lost!");
                // If game lost, announce the loss and assign the punishment role
                SocketTextChannel channel = this.discordSocketClient.GetChannel(StaticData.jalenZoneId) as SocketTextChannel;
                string msg = String.Format(this.lossAnnounceFmt, lastMatchInfo.summonerName, lastMatchInfo.championName, lastMatchInfo.kills, lastMatchInfo.deaths, lastMatchInfo.assists);
                channel.SendMessageAsync(msg, true);
                this.addOrRemoveRole(StaticData.summonerToDiscordMappings[lastMatchInfo.summonerName], StaticData.punishmentRoleId, true);
            } else if (lastMatchInfo.winner) {
                Console.WriteLine("The game was won!");
                // If game won, remove the punishment role
                this.addOrRemoveRole(StaticData.summonerToDiscordMappings[lastMatchInfo.summonerName], StaticData.punishmentRoleId, false);
            }

            this.lastMatchChecked = lastMatchInfo;

            return 0;
        }

        private void addOrRemoveRole(ulong userId, ulong roleId, bool add)
        {
            SocketGuild server = this.discordSocketClient.GetGuild(StaticData.serverId);
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

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            await this.Log(new LogMessage(LogSeverity.Info, messageParam.Author.Username, "message received in " + messageParam.Channel.Name));
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) || 
                    message.HasMentionPrefix(this.discordSocketClient.CurrentUser, ref argPos)) ||
                    message.Author.IsBot) {
                return;
            }

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(this.discordSocketClient, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            var result = await this.commandService.ExecuteAsync(context: context, argPos: argPos, services: null);
            if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
        }
    }
}
