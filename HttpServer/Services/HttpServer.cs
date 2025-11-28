using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using HttpServerApp.Interfaces;
using HttpServerApp.Models;
using Microsoft.Extensions.Logging;

namespace HttpServerApp.Services;

public class HttpServer(
  ILogger<HttpServer> logger, 
  IStreamParserFactory parserFactory, 
  IHttpFactory httpFactory,
  IRouter router
) : IHttpServer
{
  TcpListener? server;
  private readonly ILogger<HttpServer> _logger = logger;
  private readonly IStreamParserFactory _parserFactory = parserFactory;
  private readonly IHttpFactory _httpFactory = httpFactory;
  public IRouter Router {get;} = router;
  public async Task Start(IPAddress iPAddress, int port)
  {
    if (server == null)
    {
      server = new TcpListener(iPAddress, port);
    }
    try
    {
      server.Start();
      _logger.LogInformation("Server started on {Address}:{Port}", iPAddress, port);

      while (true)
      {
        _logger.LogInformation("Waiting for a connection");
        using TcpClient client = await server.AcceptTcpClientAsync();
        await ProcessClient(client);
      }
    }
    catch (SocketException e)
    {
      _logger.LogError(e, "Socket Exception occurred");
    }
    finally
    {
      server.Stop();
      _logger.LogInformation("Server stopped");
    }
  }

  public void Stop()
  {
    throw new NotImplementedException();
  }
  public async Task ProcessClient(TcpClient client)
  {
    _logger.LogInformation("Client connected");
    NetworkStream networkStream = client.GetStream();
    var req = await _httpFactory.GetRequestFromStream(networkStream);
    _logger.LogDebug($"Request: {req}");

    var res = new HttpResponse();
    // {
    //   StatusLine = new(),
    //   Headers = new()
    //   {
    //     {"Connection", "close"},
    //     {"Content-Type", "text/plain"},
    //   },
    //   Body = req.Body,
    // };
    var route = Router.FindRoute(req.RequestLine.Method.ToString(), req.RequestLine.RequestTarget);
    if(route == null)
    {
      res.StatusLine.StatusCode = "400";
      res.StatusLine.ReasonPhrase = "Bad Request";
    }
    else
    {
      route.Handler.Invoke(req, res);
    }
    _logger.LogDebug($"Response: {res}");
    await networkStream.WriteAsync(Encoding.UTF8.GetBytes(res.ToString()));

  }
}
