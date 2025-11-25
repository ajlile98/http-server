using System;
using System.Net;
using HttpServerApp.Interfaces;

namespace HttpServerApp.Services;

public class HttpServer : IHttpServer
{
  public async Task Start(IPAddress iPAddress, int port)
  {
    await Task.Delay(0); // Placeholder
  }

  public void Stop()
  {
    throw new NotImplementedException();
  }
}
