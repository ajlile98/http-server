using System.Net;
using HttpServerApp.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HttpServerApp.Interfaces;

// void GetHttpHandler(HttpRequest req, HttpResponse res)
// {
//     Console.WriteLine("Http Request!");
//     Console.WriteLine($"Method: {req.Method}");
//     Console.WriteLine($"Path: {req.Path}");
//     Console.WriteLine($"Headers:");
//     foreach (var header in req.Headers)
//     {
//         Console.WriteLine($"  {header.Key}: {header.Value}");
//     }
//     Console.WriteLine($"Body: {req.Body}");

//     res.StatusCode = 200;
//     res.StatusMessage = "OK";
//     res.Headers["Content-Type"] = "text/plain";
//     res.Body = req.Body;
// }

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddLogging(logging =>
    {
        logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = false;
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            options.UseUtcTimestamp = false;
        });
        logging.SetMinimumLevel(LogLevel.Debug);
    });
    
    services.AddSingleton<IStreamParserFactory, StreamParserFactory>();
    services.AddSingleton<IHttpFactory, HttpFactory>();
    services.AddSingleton<IHttpServer, HttpServer>();
});

var host = builder.Build();

// DI automatically creates HttpServer with its logger dependency
var server = host.Services.GetRequiredService<IHttpServer>();
await server.Start(IPAddress.Parse("127.0.0.1"), 8000);