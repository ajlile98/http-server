namespace http_server.Models;

public class HttpResponse
{
  public string HttpVersion { get; set; } = "HTTP/1.1";
  public int StatusCode { get; set; } = 200;
  public string StatusMessage { get; set; } = "Ok";
  public Dictionary<string, string> Headers { get; set; } = new();
  public string? Body { get; set; }
  public override string ToString()
  {
    string startLine = $"{HttpVersion} {StatusCode} {StatusMessage}";
    string fieldLine = "";
    string? messageBody = Body;
    foreach(var header in Headers)
    {
      fieldLine += $"{header.Key}: {header.Value}";
    }
    string httpResponseString = $"{startLine}\r\n{fieldLine}\r\n\r\n";
    if(messageBody != null)
    {
      httpResponseString += messageBody;
    }
    return httpResponseString;
  }
}
