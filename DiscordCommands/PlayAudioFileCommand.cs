using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using Discord.Commands;
using Discord.Audio;
using Discord;

namespace DiscordCommands
{
    public class PlayAudioFileCommand : ModuleBase<SocketCommandContext>
    {
        private static string downloadedFileDir = "C:\\Users\\David Fei\\Documents\\YetAnotherStupidDiscordBot\\audiofiles\\";

        [Command("playfile", RunMode = RunMode.Async)]
        [Summary("Plays an audio file in voice")]
        public async Task JoinChannelAndPlayAudio(IVoiceChannel channel = null)
        {
            string downloadedFileName = null;
            // Attempt to download the attachment if it exists
            if (Context.Message.Attachments != null && Context.Message.Attachments.Count > 0) {
                downloadedFileName = this.downloadAttachedFiles(Context.Message);
            } else {
                await Context.Channel.SendMessageAsync("You must attach a valid audio file to be played.");
                return;
            }

            if (downloadedFileName == null) return;

            // Get the audio channel
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;

            if (channel == null) {
                await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
                return;
            }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await channel.ConnectAsync(true, false);
            await this.SendAsync(audioClient, PlayAudioFileCommand.downloadedFileDir + downloadedFileName);
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

        private string downloadAttachedFiles(IMessage message)
        {
            // Get the attached audio files
            var attachments = Context.Message.Attachments;
            var file = attachments.ElementAt(0);
            WebClient myWebClient = new WebClient();

            Console.WriteLine("Audio file {0} attached: {1}",  file.Filename, file.Url);
            try {
                myWebClient.DownloadFile(file.Url, PlayAudioFileCommand.downloadedFileDir + file.Filename);
                return file.Filename;
            } catch(Exception ex) {
                Console.WriteLine(ex);
                Context.Channel.SendMessageAsync("Could not download attachment.  Please try again.");
                return null;
            }
        }
    }
}