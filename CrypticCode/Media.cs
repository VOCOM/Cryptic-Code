namespace CrypticCode;

public class Media {
  public string Title = string.Empty;
  public string URL = string.Empty;
  public Task<string>? CachedFile = null;
}
