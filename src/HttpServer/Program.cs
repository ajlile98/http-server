using System.Net;
using HttpServerApp.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HttpServerApp.Interfaces;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateDefaultBuilder(args)
    .UseContentRoot(AppContext.BaseDirectory);

builder.ConfigureAppConfiguration((context, config) =>
{
    config.SetBasePath(context.HostingEnvironment.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
          .AddEnvironmentVariables()
          .AddCommandLine(args);
});

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
    services.AddSingleton<IRouter, Router>();
});

var host = builder.Build();

var config = host.Services.GetRequiredService<IConfiguration>();

Console.WriteLine($"Config loaded - Host: {config["Server:Host"]}, Port: {config["Server:Port"]}");

// DI automatically creates HttpServer with its logger dependency
var server = host.Services.GetRequiredService<IHttpServer>();
// Add routes to the server's router
server.Router.Get("/", (req, res) =>
{
    res.StatusLine.StatusCode = "200";
    res.Body = "Hello World";
    res.Headers["Content-Type"] = "text/plain";
});

server.Router.Post("/api/users", (req, res) =>
{
    res.StatusLine.StatusCode = "201";
    res.Body = req.Body;
});

var iPAddress = config["Server:Host"] ?? throw new InvalidOperationException("Server:Host configuration is required");
var port = int.Parse(config["Server:Port"] ?? throw new InvalidOperationException("Server:Port configuration is required"));

await server.Start(IPAddress.Parse(iPAddress), port);