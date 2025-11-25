using System;
using System.Net;

namespace HttpServerApp.Interfaces;

public interface IHttpServer
{
  public Task Start(IPAddress iPAddress, int port);
  public void Stop();
}
