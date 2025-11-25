using System;
using System.Text;

namespace HttpServerApp.Services;

public class StreamParser
{
  private readonly Stream _stream;
  private readonly int _bufferSize;
  private readonly char _lineDelimiter;
  private string _remainingData = string.Empty;

  public StreamParser(Stream stream, int bufferSize = 8192, char lineDelimiter = '\n')
  {
    _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    _bufferSize = bufferSize;
    _lineDelimiter = lineDelimiter;
  }
  public string ParseLine()
  {
    var buffer = new byte[_bufferSize];
    while(true){
      var lineEnd = _remainingData.IndexOf(_lineDelimiter);
      if(lineEnd != -1) // lineDelimiter found
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
}
