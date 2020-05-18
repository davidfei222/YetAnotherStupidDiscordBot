using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Objects;

namespace DiscordCommands
{
    public class TestCommand : ModuleBase<SocketCommandContext>
    {
        [Command("punishjalen")]
	    [Summary("Assigns Jalen the punishment role for test purposes")]
        public async Task PunishJalen()
        {
            SocketRole punishmentRole = this.Context.Guild.GetRole(StaticData.punishmentRoleId); 
            SocketGuildUser user = this.Context.Guild.GetUser(StaticData.summonerToDiscordMappings["AnimePenguin"]);
            await user.AddRoleAsync(punishmentRole);
        }

        [Command("unpunishjalen")]
	    [Summary("Removes the punishment role from Jalen for test purposes")]
        public async Task UnPunishJalen()
        {
            SocketRole punishmentRole = this.Context.Guild.GetRole(StaticData.punishmentRoleId); 
            SocketGuildUser user = this.Context.Guild.GetUser(StaticData.summonerToDiscordMappings["AnimePenguin"]);
            await user.RemoveRoleAsync(punishmentRole);
        }
    }
}