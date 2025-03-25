using Discord;
using Discord.WebSocket;

namespace CrypticCode;

public class DiscordClient {
  public void Start() {
    client.LoginAsync(TokenType.Bot, token).Wait();
    client.StartAsync().Wait();
  }

  public DiscordClient() {
    token = Utility.GetToken("discord-token");

    DiscordSocketConfig config = new();
    config.GatewayIntents |= GatewayIntents.MessageContent;

    client = new(config);
    client.Ready += ReadyCallback;
    client.MessageReceived += MessageCallback;
  }

  async Task YoutubeSearch(IMessageChannel output, string source, string query) {
    Task<string> search = Task.Run(async () => {
      string message = string.Empty;
      string[] results = await youtubeClient.Search(query);
      for(int i = 0; i < results.Length; i++)
        message += $"{i}. {results[i]}\n";
      return message;
    });
    _ = await output.SendMessageAsync("Hi " + source);
    _ = await output.SendMessageAsync(await search);
  }

  async Task ReadyCallback() => await Task.Run(() => Console.WriteLine("Ready..."));
  async Task MessageCallback(SocketMessage msg) {
    SocketUser sender = msg.Author;
    ISocketMessageChannel channel = msg.Channel;

    // Recursion guard
    if(sender.Username == "Cryptic Code") return;

    switch(channel.GetChannelType()) {
      case ChannelType.DM:
        await YoutubeSearch(await sender.CreateDMChannelAsync(), sender.Username, msg.Content);
        break;
      case ChannelType.Text:
        if(msg.Content.Length < 4) return;
        if(!msg.Content[..4].Contains(prefix)) return;
        _ = await channel.SendMessageAsync("Test reply from " + sender.Username);
        break;
      case ChannelType.Voice:
      case ChannelType.Group:
      case ChannelType.Category:
      case ChannelType.News:
      case ChannelType.Store:
      case ChannelType.NewsThread:
      case ChannelType.PublicThread:
      case ChannelType.PrivateThread:
      case ChannelType.Stage:
      case ChannelType.GuildDirectory:
      case ChannelType.Forum:
      case ChannelType.Media:
      case null:
      default:
        break;
    }
  }

  readonly string token;
  readonly DiscordSocketClient client;

  readonly YoutubeClient youtubeClient = new();

  const string prefix = "code";
}
