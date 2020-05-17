using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Discord.Commands;
using Discord;

namespace DiscordCommands
{
    public class PlayAudioFileCommand : ModuleBase<SocketCommandContext>
    {
        [Command("play", RunMode = RunMode.Async)]
        [Summary("Plays an audio file in voice")]
        public async Task JoinChannelAndPlayAudio(IVoiceChannel channel = null)
        {
            // Get the audio channel
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;

            if (channel == null) {
                await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
                return;
            }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await channel.ConnectAsync(true, false);
        }

        private void downloadAttachedFiles(IMessage message)
        {
            // Get the attached audio files
            var attachments = Context.Message.Attachments;
            WebClient myWebClient = new WebClient();

            foreach (IAttachment file in attachments) {
                Console.WriteLine("Audio file attached: " + file.ProxyUrl);
                byte[] buffer = myWebClient.DownloadData(file.ProxyUrl);
            }
        }
    }
}