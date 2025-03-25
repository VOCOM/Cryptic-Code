using YoutubeExplode.Search;

namespace CrypticCode;

public class YoutubeClient {

  public async Task<string[]> Search(string query) {
    string[] results = [];
    if(query == string.Empty) return results;

    int count = 0;
    await foreach(ISearchResult result in client.Search.GetResultsAsync(query)) {
      if(count++ >= maxResults) return results;
      results = [.. results, result.Title];
    }

    return results;
  }

  readonly YoutubeExplode.YoutubeClient client = new();
  const int maxResults = 5;
}
