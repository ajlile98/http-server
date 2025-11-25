namespace http_server.Models;

public class HttpRequest
{
  public HttpMethod Method { get; init; } = HttpMethod.Get;
  public string Path { get; init; } = "/";
  public string HttpVersion { get; init; } = "HTTP/1.1";
  public Dictionary<string, string> Headers { get; init; } = new();
  public string? Body { get; init; }
}


