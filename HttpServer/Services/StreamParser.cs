using System;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using HttpServerApp.Interfaces;
using HttpServerApp.Models;
using Microsoft.Extensions.Logging;

namespace HttpServerApp.Services;

public class StreamParser : IStreamParser
{
  private readonly Stream _stream;
  private readonly int _bufferSize;
  private readonly char _lineDelimiter;
  private string _remainingData = string.Empty;
  private ILogger<StreamParser> _logger;

  public StreamParser(Stream stream, ILogger<StreamParser> logger, int bufferSize = 8, char lineDelimiter = '\n')
  {
    _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    _bufferSize = bufferSize;
    _lineDelimiter = lineDelimiter;
    _logger = logger;
  }
  public string ParseLine()
  {
    var buffer = new byte[_bufferSize];
    while (true)
    {
      var lineEnd = _remainingData.IndexOf(_lineDelimiter);
      if (lineEnd != -1) // lineDelimiter found
      {
        var line = _remainingData.Substring(0, lineEnd);
        _remainingData = _remainingData.Substring(lineEnd + 1);
        return line;
      }

      int bytesRead = _stream.Read(buffer, 0, _bufferSize);
      if (bytesRead == 0) // client connection closed
      {
        var lastLine = _remainingData;
        _remainingData = string.Empty;
        return lastLine;

      }

      var newData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
      _remainingData += newData;

    }
  }
  private string GetBody(int contentLength)
  {
    var buffer = new byte[contentLength];
    var bodyBuilder = new StringBuilder(_remainingData);
    int totalBytesNeeded = contentLength - _remainingData.Length;
    int bytesRead = _stream.Read(buffer, 0, totalBytesNeeded);
    bodyBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
    _remainingData = string.Empty;

    return bodyBuilder.ToString();
  }

  public string[] ParseLines()
  {
    var lines = new List<string>();
    string line;

    while (!string.IsNullOrEmpty(line = ParseLine()))
    {
      lines.Add(line);
    }

    return lines.ToArray();
  }
  private int? GetContentLength(string line)
  {
    if (!line.StartsWith("Content-Length", StringComparison.OrdinalIgnoreCase))
      return null;

    var parts = line.Split(':', 2);
    if (parts.Length != 2)
      return null;

    if (int.TryParse(parts[1].Trim(), out int contentLength))
      return contentLength;

    return null;
  }
  public Channel<string> GetLinesChannel()
  {
    var requestChannel = Channel.CreateUnbounded<string>();
    _ = Task.Run(async () =>
    {
      try
      {
        string line;
        int? contentLength = null;
        while (!string.IsNullOrEmpty(line = ParseLine()))
        {
          line = line.Trim('\r');
          await requestChannel.Writer.WriteAsync(line);
          
          if (contentLength == null)
            contentLength = GetContentLength(line);
          if (string.IsNullOrEmpty(line))
          {
            break;
          }
        }

        await requestChannel.Writer.WriteAsync("");

        if (contentLength != null)
        {
          var body = GetBody((int)contentLength);
          await requestChannel.Writer.WriteAsync(body);
        }

      }
      finally
      {
        requestChannel.Writer.Complete();
      }
    });
    return requestChannel;
  }

}