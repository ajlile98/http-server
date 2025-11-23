using System;
using System.Text;
using http_server.Interfaces;

namespace http_server.Services;

public class ByteStreamParser : IStreamParser
{
  public List<string> GetLines(Stream s)
  {
    using (BinaryReader br = new BinaryReader(s, Encoding.UTF8, leaveOpen: true))
    {
      var reader = new HttpBinaryReader(br, bufferSize: 8);
      
      // Read headers until empty line
      var lines = reader.ReadUntilEmptyLine(out int? contentLength);
      
      // Read body if present
      if (contentLength.HasValue && contentLength.Value > 0)
      {
        var bodyLines = reader.ReadBody(contentLength.Value);
        lines.AddRange(bodyLines);
      }
      
      return lines;
    }
  }

}