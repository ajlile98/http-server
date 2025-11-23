namespace http_server.Interfaces;

public interface IStreamParser
{
  List<string> GetLines(Stream s);
  // public string ReadRequestLine(out HttpMethod httpMethod, out string path, out string version);
}
