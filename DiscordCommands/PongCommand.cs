using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordCommands
{
    public class PongCommand : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
	    [Summary("Responds to ping with pong")]
        public async Task PongAsync() => await ReplyAsync("Pong!");
    }
}