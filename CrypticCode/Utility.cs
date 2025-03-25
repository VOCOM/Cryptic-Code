using System.Reflection;

namespace CrypticCode;

public static class Utility {
  public static string GetToken(string key) {
    Stream? stream = Assembly
      .GetExecutingAssembly()
      .GetManifestResourceStream("CrypticCode..env")
      ?? throw new DirectoryNotFoundException();
    string env = new StreamReader(stream)
      .ReadToEnd();

    int index = env.IndexOf(key);
    if(index < 0) return string.Empty;

    int startIndex = env.IndexOf('\"', index) + 1;
    int endIndex = env.IndexOf('\"', startIndex);
    return env[startIndex..endIndex];
  }
}
