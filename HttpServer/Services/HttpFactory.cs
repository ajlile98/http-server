using System.Threading.Tasks;
using HttpServerApp.Interfaces;
using HttpServerApp.Models;
using Microsoft.Extensions.Logging;
using HttpMethod = HttpServerApp.Models.HttpMethod;

namespace HttpServerApp.Services;

public class HttpFactory(ILogger<HttpFactory> logger, IStreamParserFactory streamParserFactory) : IHttpFactory
{
  ILogger<HttpFactory> _logger = logger;
  IStreamParserFactory _streamParserFactory = streamParserFactory;
  public async Task<HttpRequest> GetRequestFromStream(Stream stream)
  {
    var streamParser = _streamParserFactory.Create(stream);
    var channel = streamParser.GetLinesChannel();

    var method = "";
    var requestTarget = "";
    var httpVersion = "";
    Dictionary<string, string> headers = new();
    string body = "";
    var state = ParserState.REQUEST_LINE;

    await foreach (var line in channel.Reader.ReadAllAsync())
    {
      switch (state)
      {
        case ParserState.REQUEST_LINE:
          var requestLine = line.Split();
          method = requestLine[0];
          requestTarget = requestLine[1];
          httpVersion = requestLine[2].Replace("HTTP/", "");
          state = ParserState.HEADERS;
          break;
        case ParserState.HEADERS:
          if (string.IsNullOrWhiteSpace(line))
          {
            state = ParserState.BODY;
          }
          else
          {
            var trimmedLine = line.Trim();

            int colonIndex = trimmedLine.IndexOf(": ", StringComparison.Ordinal);
            if (colonIndex == -1)
            {
              _logger.LogError("Malformed header: {Header}", line);
              throw new FormatException($"Malformed header: {line}");
            }

            var headerKey = trimmedLine.AsSpan(0, colonIndex);

            if (headerKey.Length > 0 && char.IsWhiteSpace(headerKey[^1]))
            {
              _logger.LogError("Malformed header (space before colon): {Header}", line);
              throw new FormatException($"Malformed header (space before colon): {line}");
            }

            // Validate header key contains only valid token characters
            // RFC 9110: token = 1*tchar, tchar = visible ASCII except delimiters
            foreach (char c in headerKey)
            {
              if (c < 33 || c > 126 || "\"(),/:;<=>?@[\\]{}".Contains(c))
              {
                _logger.LogError("Malformed header (invalid character in key): {Header}", line);
                throw new FormatException($"Malformed header (invalid character in key): {line}");
              }
            }

            var headerValue = trimmedLine.AsSpan(colonIndex + 2);
            var headerKeyString = headerKey.ToString();

            if (headers.ContainsKey(headerKeyString))
            {
              headers[headerKeyString] = headers[headerKeyString] + ", " + headerValue.ToString();
            }
            else
            {
              headers.Add(headerKeyString, headerValue.ToString());
            }
          }
          break;
        case ParserState.BODY:
          body += line;
          break;
        default:
          _logger.LogWarning("Default Case Found in HTTP Parser");
          break;
      }
    }
    return new HttpRequest
    {
      RequestLine = new()
      {
        Method = new HttpMethod(method),
        RequestTarget = requestTarget,
        HttpVersion = httpVersion,
      },
      Headers = headers,
      Body = body,
    };
  }

}
