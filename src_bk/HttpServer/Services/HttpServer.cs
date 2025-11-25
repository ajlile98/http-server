using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using http_server.Factories;
using http_server.Interfaces;
using http_server.Models;
using http_server.Services;

namespace http_server.Listeners;

public class HttpServer : IHttpServer
{
  TcpListener? server;
  Dictionary<String, Action<HttpRequest, HttpResponse>> Routes { get; set; } = new();
  private readonly IStreamParser _parser;

  public HttpServer(IStreamParser? parser = null)
  {
    if (parser == null)
    {
      parser = new ByteStreamParser();
    }
    _parser = parser;
  }


  public async Task Start(IPAddress ip, int port)
  {
    if (server == null)
    {
      server = new TcpListener(ip, port);
    }
    try
    {
      server.Start();

      while (true)
      {
        Console.WriteLine("Waiting for a connection");
        using TcpClient client = await server.AcceptTcpClientAsync();
        await ProcessClient(client);
      }
    }
    catch (SocketException e)
    {
      Console.WriteLine("Socket Exception: {0}", e);
    }
    finally
    {
      server.Stop();
    }

    Console.WriteLine("\nHit enter to continue...");
    Console.Read();

  }

  protected async Task ProcessClient(TcpClient client)
  {
    Console.WriteLine("Connected!");
    var res = new HttpResponse();
    NetworkStream ns = client.GetStream();
    ns.ReadTimeout = 5000;
    try
    {

      var lines = _parser.GetLines(ns);
      foreach (var line in lines)
      {
        Console.WriteLine(line);
      }

      var req = RequestFactory.GetRequest(lines.ToArray());
      // var req = new HttpRequest();
      Routes[req.Path].Invoke(req, res);


    } 
    catch (System.FormatException e)
    {
      Console.WriteLine($"Error Processing Data from HTTP Request: {e}");
      res.StatusCode = 400;
      res.StatusMessage = "Bad Request";
    }
    catch (System.IO.IOException e)
    {
      Console.WriteLine($"Error Timeout Exception {e}");
      res.StatusCode = 400;
      res.StatusMessage = "Bad Request";
    }
    finally
    {
      await SendResponse(res, ns);
    }

  }

  private async Task SendResponse(HttpResponse res, NetworkStream ns)
  {
      string httpResponseString = res.ToString();
      Console.WriteLine($"httpResponse:\n{httpResponseString}");
      byte[] buffer = Encoding.UTF8.GetBytes(httpResponseString);
      await ns.WriteAsync(buffer, 0, buffer.Length);
      await ns.FlushAsync();
      Console.WriteLine("Response sent");
  }

  public void Stop()
  {
    server?.Stop();
  }

  public void RegisterRoute(string path, Action<HttpRequest, HttpResponse> handler)
  {
    Routes.Add(path, handler);
  }
}

