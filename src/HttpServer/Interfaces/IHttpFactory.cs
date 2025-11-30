using System;
using HttpServerApp.Models;

namespace HttpServerApp.Interfaces;

public interface IHttpFactory
{
  public Task<HttpRequest> GetRequestFromStream(Stream stream);
}
