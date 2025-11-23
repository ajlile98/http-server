using http_server.Models;

namespace http_server.Factories;

public class RequestFactory
{
  public static HttpRequest GetRequest(string[] lines)
  {
    foreach (var line in lines)
    {
      Console.WriteLine(line);
    }
    var split_first_line = lines[0].Split();
    var method = split_first_line[0];
    var path = split_first_line[1];
    var version = split_first_line[2];
    var headers = new Dictionary<string, string>();
    string? body = null;
    foreach (var line in lines.Skip(1))
    {
      if (string.IsNullOrEmpty(line.Trim()))
      {
        body = "";
        continue;
      }
      if (body == null)
      {
        var split_line = line.Split(": ", 2);
        if (split_line.Length >= 2)
        {
          headers.Add(split_line[0].Trim(), split_line[1].Trim());
        }
      }
      else
      {
        body += line + "\n";
      }
    }

    return new HttpRequest
    {
      Method = HttpMethod.Parse(method),
      Path = path,
      HttpVersion = version,
      Headers = headers,
      Body = body,
    };

  }
}
