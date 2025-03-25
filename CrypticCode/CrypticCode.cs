namespace CrypticCode;

public static class CrypticCode {
  static void Main() {
    discordClient.Start();

    while(true) ;
  }

  static readonly DiscordClient discordClient = new();
}