using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Discord.Commands;
using Discord.Audio;
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
            await this.SendAsync(audioClient, "C:\\Users\\David Fei\\Documents\\YetAnotherStupidDiscordBot\\audiofiles\\peanutbutter.mp3");
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
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