using System.Globalization;
using System.Text;

namespace http_server.Services;

public class HttpBinaryReader
{
  private readonly BinaryReader _reader;
  private readonly int _bufferSize;
  private string _currentLine = "";

  public HttpBinaryReader(BinaryReader reader, int bufferSize = 8)
  {
    _reader = reader;
    _bufferSize = bufferSize;
  }

  /// <summary>
  /// Reads lines from the stream until an empty line is found or stream ends.
  /// Returns all complete lines including the empty line marker.
  /// </summary>
  public List<string> ReadUntilEmptyLine(out int? contentLength)
  {
    List<string> lines = new List<string>();
    contentLength = null;
    bool foundEmptyLine = false;

    while (!foundEmptyLine)
    {
      byte[] buffer = _reader.ReadBytes(_bufferSize);
      if (buffer.Length == 0) break;

      string bufferString = Encoding.UTF8.GetString(buffer);
      string[] splitLines = bufferString.Split('\n');

      // Process all complete lines in this buffer
      for (int i = 0; i < splitLines.Length - 1; i++)
      {
        string part = splitLines[i].TrimEnd('\r');
        string completedLine = (i == 0 ? _currentLine.TrimEnd('\r') + part : part);
        lines.Add(completedLine);

        // Check for Content-Length header
        if (completedLine.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
        {
          contentLength = ParseContentLength(completedLine);
        }

        // Check for empty line marking end of headers
        if (string.IsNullOrEmpty(completedLine))
        {
          foundEmptyLine = true;
          _currentLine = splitLines[splitLines.Length - 1]; // Save any remaining data
          break; // Stop processing this buffer
        }

        // Reset _currentLine after processing first element
        if (i == 0)
        {
          _currentLine = "";
        }
      }

      // Only update _currentLine if we haven't found empty line yet
      if (!foundEmptyLine)
      {
        // Last element becomes the new current line (might be incomplete)
        _currentLine += splitLines[splitLines.Length - 1];
      }
    }

    // Don't add remaining content to lines if we found empty line
    // (it's the start of the body, not a header)
    if (!foundEmptyLine && !string.IsNullOrEmpty(_currentLine))
    {
      lines.Add(_currentLine.TrimEnd('\r'));
      _currentLine = "";
    }

    return lines;
  }

  /// <summary>
  /// Reads exactly the specified number of bytes and splits into lines.
  /// </summary>
  public List<string> ReadBody(int contentLength)
  {
    if (contentLength <= 0) return new List<string>();

    // Account for any partial data already in _currentLine from the headers
    string bodyData = _currentLine;
    int remainingBytes = contentLength - Encoding.UTF8.GetByteCount(bodyData);

    if (remainingBytes > 0)
    {
      byte[] bodyBytes = _reader.ReadBytes(remainingBytes);
      bodyData += Encoding.UTF8.GetString(bodyBytes);
    }

    _currentLine = ""; // Clear after consuming

    string[] bodyLines = bodyData.Split('\n');
    List<string> lines = new List<string>();

    foreach (var line in bodyLines)
    {
      lines.Add(line.TrimEnd('\r'));
    }

    return lines;
  }

  private int? ParseContentLength(string line)
  {
    var parts = line.Split(':', 2);
    if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int length))
    {
      return length;
    }
    return null;
  }
}
