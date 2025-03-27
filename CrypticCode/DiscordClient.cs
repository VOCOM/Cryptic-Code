using Discord;
using Discord.WebSocket;
using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace CrypticCode;

public class DiscordClient {
  public void Start() {
    dClient.LoginAsync(TokenType.Bot, token).Wait();
    dClient.StartAsync().Wait();
    playlist.Start();
  }
  public void Stop() {
    playlist.Stop();
    dClient.StopAsync().Wait();
  }

  public DiscordClient() {
    token = Utility.GetToken("discord-token");

    DiscordSocketConfig config = new();
    config.GatewayIntents |= GatewayIntents.MessageContent;

    dClient = new(config);
    dClient.Ready += ReadyCallback;
    dClient.MessageReceived += MessageCallback;
  }

  // Youtube services
  async Task<List<Media>> Search(string query) {
    List<Media> results = [];
    await foreach(ISearchResult result in ytClient.Search.GetVideosAsync(query)) {
      results.Add(new Media() { Title = result.Title, URL = result.Url });
      if(results.Count >= maxResults) break;
    }
    return results;
  }
  async Task<string> DownloadAudio(Media video) {
    StreamManifest streamManifest = await ytClient.Videos.Streams.GetManifestAsync(video.URL);
    IStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

    Directory.CreateDirectory("cache");
    string file = $"{Random.Shared.Next()}.webm";
    await ytClient.Videos.Streams.DownloadAsync(streamInfo, file);
    return file;
  }

  // Callbacks
  async Task ReadyCallback() => await Task.Run(() => Console.WriteLine("Ready..."));
  async Task MessageCallback(SocketMessage msg) {
    SocketUser sender = msg.Author;
    if(sender.IsBot || sender.IsWebhook) return;
    string content = msg.CleanContent;

    // Prefix reset / guard
    if(content.StartsWith(prefix)) state = State.Idle;
    else if(state is State.Idle) return;

    ISocketMessageChannel channel = msg.Channel;
    switch(channel.GetChannelType()) {
      case ChannelType.Text:
        switch(state) {
          case State.Idle:
            // Strip prefix
            content = content[(content.IndexOf(' ') + 1)..];

            // Resolve command
            bool play = content[..content.IndexOf(' ')]
              .Contains("play", StringComparison.CurrentCultureIgnoreCase);

            // Strip command
            content = content[(content.IndexOf(' ') + 1)..];
            if(content.Length == 0) return;

            uint i = 0;
            string results = $"Search results for {content}\n";
            foreach(Media result in searchResults = await Search(content))
              results += $"{++i}. {result.Title}\n";
            await Repond(channel, results);

            state = State.Select;
            break;
          case State.Select:
            // Don't clear error messages
            if(content.Any(char.IsLetter) || Convert.ToInt32(content) > searchResults.Count) {
              _ = await channel.SendMessageAsync("Invalid option, please choose again.");
              return;
            }

            int idx = Convert.ToInt32(content);
            Media media = searchResults[idx - 1];
            media.CachedFile = DownloadAudio(media);
            SocketGuildUser? user = FindUser(sender.MutualGuilds, sender.Id);

            // Don't clear error messages
            if(user?.VoiceChannel is null) {
              _ = await channel.SendMessageAsync("Please enter a voice channel before queuing a song.");
              return;
            }

            await Repond(channel, $"Playing: {media.Title}");
            playlist.Channel = user.VoiceChannel;
            playlist.Add(media);
            break;
        }
        break;
      default: break;
    }
  }

  // Helper
  async Task Repond(IMessageChannel channel, string response) {
    if(lastResponse is not null)
      await lastResponse.DeleteAsync();
    if(channel is not null)
      lastResponse = await channel.SendMessageAsync(response);
  }
  static SocketGuildUser? FindUser(IReadOnlyCollection<SocketGuild> mutualGuilds, ulong userId) {
    SocketGuild? guild = mutualGuilds.First(guild => guild.GetUser(userId) is not null);
    return guild?.GetUser(userId);
  }

  public enum State {
    Idle,
    Select,
  }
  public enum Command {
    Unknown,
    Play,
  }

  State state = State.Idle;
  IUserMessage? lastResponse;
  List<Media> searchResults = [];

  readonly string token;
  readonly Playlist playlist = new();
  readonly DiscordSocketClient dClient;
  readonly YoutubeClient ytClient = new();

  const string prefix = "code";
  const int maxResults = 5;
}
