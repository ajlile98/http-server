using System;
using System.Net;
using http_server.Models;

namespace http_server.Interfaces;

public interface IHttpServer
{
    Task Start(IPAddress ip, int port);
    void Stop();
    void RegisterRoute(string path, Action<HttpRequest, HttpResponse> handler);
}
