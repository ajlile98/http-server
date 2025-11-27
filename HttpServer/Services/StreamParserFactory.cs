using System;
using HttpServerApp.Interfaces;
using Microsoft.Extensions.Logging;

namespace HttpServerApp.Services;

public class StreamParserFactory(ILogger<StreamParser> logger) : IStreamParserFactory
{
  private readonly ILogger<StreamParser> _logger = logger;
  public IStreamParser Create(Stream stream)
  {
    return new StreamParser(stream, _logger);
  }
}
