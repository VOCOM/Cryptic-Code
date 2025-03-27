using Discord.Audio;
using Discord.WebSocket;
using FFMpegCore;
using FFMpegCore.Pipes;

namespace CrypticCode;

public class Playlist {
  public void Start() => handle.Start();
  public void Stop() => source.Cancel();

  public void Add(Media song) => playlist.Enqueue(song);

  public Playlist() => handle = new(Loop);

  async void Loop() {
    while(source.IsCancellationRequested is false) {
      if(playlist.Count == 0) continue;
      if(Channel is null) continue;

      Media audio = playlist.Dequeue();
      if(audio.CachedFile is null) continue;

      using Task<string> filename = audio.CachedFile;
      using Task<IAudioClient> audioClient = Channel.ConnectAsync();
      using AudioOutStream discordStream = (await audioClient).CreateDirectPCMStream(AudioApplication.Music);
      StreamPipeSink outputStream = new(discordStream);

      bool a = await FFMpegArguments
        .FromFileInput(await filename)
        .OutputToPipe(outputStream, options => options
          .WithCustomArgument("-ac 2")
          .WithAudioCodec("pcm_s16le")
          .WithAudioSamplingRate(48000)
          .ForceFormat("wav"))
        .ProcessAsynchronously();
    }
  }

  public SocketVoiceChannel? Channel { get; set; } = null;

  readonly Task handle;
  readonly Queue<Media> playlist = [];
  readonly CancellationTokenSource source = new();
}
