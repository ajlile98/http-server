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
    
    // Request line
    sb.AppendLine($"{RequestLine.Method} {RequestLine.RequestTarget} HTTP/{RequestLine.HttpVersion}");
    
    // Headers
    foreach (var header in Headers)
    {
      sb.AppendLine($"{header.Key}: {header.Value}");
    }
    
    // Empty line separating headers from body
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
  public string Method {get; set;} = "GET";
  public string RequestTarget {get;set;} = "/";
}