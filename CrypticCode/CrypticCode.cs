namespace CrypticCode;

public static class CrypticCode {
  static void Main() {
    discordClient.Start();

    while(true) {
      if(Console.ReadLine() == "exit") break;
    }

    discordClient.Stop();
  }

  static readonly DiscordClient discordClient = new();
}