using System.Net;
using http_server.Listeners;
using http_server.Models;

void GetHttpHandler(HttpRequest req, HttpResponse res)
{
  Console.WriteLine("Http Request!");
  Console.WriteLine($"Method: {req.Method}");
  Console.WriteLine($"Path: {req.Path}");
  Console.WriteLine($"Headers:");
  foreach(var header in req.Headers)
  {
    Console.WriteLine($"  {header.Key}: {header.Value}");
  }
  Console.WriteLine($"Body: {req.Body}");

  res.StatusCode = 200;
  res.StatusMessage = "OK";
  res.Headers["Content-Type"] = "text/plain";
  res.Body = "Hello from the handler!";
  
  // res.StatusCode = 401;
  // res.StatusMessage = "Forbidden";
  // res.Headers["Content-Type"] = "application/json";
  // res.Body = "{\"message\":\"Hello from the handler! (restricted area)\"}";
}

HttpServer server = new HttpServer();

server.RegisterRoute("/", GetHttpHandler);
await server.Start(IPAddress.Parse("127.0.0.1"), 8000);