using System;
using System.Data;
using System.Text;

namespace HttpServerApp.Models;

public class HttpRequest
{
  public RequestLine RequestLine {get; set;} = new();
  public Dictionary<string, string> Headers {get; set;} = new(StringComparer.OrdinalIgnoreCase);
  public string Body {get; set;} = "";

  public override string ToString()
  {
    var sb = new StringBuilder();
    
    sb.AppendLine($"{RequestLine.Method} {RequestLine.RequestTarget} HTTP/{RequestLine.HttpVersion}");
    
    foreach (var header in Headers)
    {
      sb.AppendLine($"{header.Key}: {header.Value}");
    }
    
    if (!string.IsNullOrEmpty(Body))
    {
      sb.AppendLine();
      sb.Append(Body);
    }
    
    return sb.ToString();
  }
}

public class RequestLine
{
  public string HttpVersion {get; set;} = "1.1";
  public HttpMethod Method {get; set;} = HttpMethod.Get;
  public string RequestTarget {get;set;} = "/";
}