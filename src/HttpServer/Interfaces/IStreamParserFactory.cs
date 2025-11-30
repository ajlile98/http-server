using System;

namespace HttpServerApp.Interfaces;

public interface IStreamParserFactory
{
  public IStreamParser Create(Stream stream);
}
