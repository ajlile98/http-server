using System;
using System.Text;

namespace HttpServerApp.Models;

public class HttpResponse
{
  public StatusLine StatusLine {get; set;} = new();
  public Dictionary<string, string> Headers = new(StringComparer.OrdinalIgnoreCase);
  public string Body = "";

  public override string ToString()
  {
    var sb = new StringBuilder();
    
    // Status line
    sb.AppendLine($"{StatusLine.HttpVersion} {StatusLine.StatusCode} {StatusLine.ReasonPhrase}");
    
    // Headers
    foreach (var header in Headers)
    {
      sb.AppendLine($"{header.Key}: {header.Value}");
    }
    
    // Empty line separating headers from body (ALWAYS required)
    sb.AppendLine();
    
    // Body (optional)
    if (!string.IsNullOrEmpty(Body))
    {
      sb.Append(Body);
    }
    
    return sb.ToString();
  }
}
public class StatusLine
{
  public string HttpVersion {get; set;} = "HTTP/1.1";
  public string StatusCode {get; set;} = "200";
  public string ReasonPhrase {get; set;} = "Ok";
}

public enum StatusCode
{
  OK = 200,
  BadRequest = 400,
  NotFound = 404,
  InternalServerError = 500
}