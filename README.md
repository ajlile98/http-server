# HTTP Server

A demo HTTP server implementation in C# demonstrating modern design patterns, dependency injection, and RFC 9110-compliant HTTP request parsing. Built with .NET 10 using clean architecture principles.

## Overview

This project implements a fully-featured HTTP server from scratch, showcasing:
- RFC 9110 compliant HTTP header parsing with strict validation
- Async/await patterns for high-performance request handling
- Factory pattern for object creation and dependency injection
- Router pattern for flexible request routing
- Comprehensive test coverage with 67+ unit tests

## Project Structure

```
http-server/
├── Models/                    # Data models
│   ├── HttpRequest.cs         # Request with RequestLine and headers
│   ├── HttpResponse.cs        # Response with StatusLine and auto Content-Length
│   ├── HttpMethod.cs          # Strongly-typed HTTP method enumeration
│   ├── Route.cs               # Route handler with method, path, and delegate
│   ├── ParserState.cs         # State machine for HTTP parsing
│   └── RequestLine.cs         # HTTP request line (method, target, version)
├── Interfaces/                # Contract definitions
│   ├── IHttpServer.cs         # Server interface with routing
│   ├── IRouter.cs             # Route management and lookup
│   ├── IHttpFactory.cs        # HTTP request parsing
│   ├── IStreamParser.cs       # Low-level stream reading
│   └── IStreamParserFactory.cs # Stream parser creation
├── Services/                  # Core implementation
│   ├── HttpServer.cs          # Main TCP server and client handling
│   ├── Router.cs              # Nested Dictionary routing with ANY wildcard
│   ├── HttpFactory.cs         # RFC 9110 compliant HTTP parser
│   ├── StreamParser.cs        # Async line-based stream reading
│   └── StreamParserFactory.cs # Factory for stream parsers
├── Test/                      # (Currently empty, tests in separate project)
├── Program.cs                 # Dependency injection and app entry point
└── HttpServer.csproj          # Project configuration
```

## Architecture

### Dependency Injection
Uses Microsoft.Extensions.DependencyInjection with ASP.NET Core style service registration:

```csharp
services.AddSingleton<IStreamParserFactory, StreamParserFactory>();
services.AddSingleton<IHttpFactory, HttpFactory>();
services.AddSingleton<IRouter, Router>();
services.AddSingleton<IHttpServer, HttpServer>();
```

### Factory Pattern
- **StreamParserFactory**: Creates per-connection stream parsers
- **HttpFactory**: Parses raw TCP streams into structured HttpRequest objects

### Router Pattern
Nested Dictionary-based routing with support for:
- Method-specific routes (GET /api/users)
- ANY wildcard routes that match all methods
- Case-insensitive HTTP method matching
- Priority system (specific methods override ANY)

```csharp
router.Get("/api/users", (req, res) => {
    res.Body = "GET /api/users handler";
    res.StatusLine.StatusCode = "200";
});

router.Any("/status", (req, res) => {
    res.Body = "Status OK";
});
```

## Key Components

### HttpRequest & HttpResponse
Type-safe models representing HTTP concepts:
- **HttpRequest**: Contains RequestLine (method, target, version) and headers
- **HttpResponse**: Contains StatusLine (version, code, phrase), headers, and body
- Automatic Content-Length calculation based on UTF-8 encoded body

### HttpMethod
Strongly-typed enumeration with built-in equality comparison:
```csharp
public static readonly HttpMethod Get = new("GET");
public static readonly HttpMethod Post = new("POST");
public static readonly HttpMethod Put = new("PUT");
public static readonly HttpMethod Delete = new("DELETE");
```

### HttpFactory
RFC 9110 compliant parser featuring:
- Header key validation (RFC token format)
- Multi-value header support (CSV concatenation)
- Case-insensitive header storage
- Whitespace handling per RFC 9110 (OWS = optional whitespace)
- State machine parsing (REQUEST_LINE → HEADERS → BODY)

### Router
Manages HTTP route registration and matching:
- Nested Dictionary structure: `Dictionary<method, Dictionary<path, Route>>`
- Case-insensitive method matching
- Wildcard ANY method for catch-all routes
- Handler execution: `route.Handler.Invoke(request, response)`

## Usage Example

```csharp
using System.Net;
using HttpServerApp.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Setup dependency injection
var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<IStreamParserFactory, StreamParserFactory>();
    services.AddSingleton<IHttpFactory, HttpFactory>();
    services.AddSingleton<IRouter, Router>();
    services.AddSingleton<IHttpServer, HttpServer>();
});

var host = builder.Build();

// Get server instance from DI
var server = host.Services.GetRequiredService<IHttpServer>();

// Register GET route
server.Router.Get("/", (req, res) =>
{
    res.StatusLine.StatusCode = "200";
    res.Headers["Content-Type"] = "text/plain";
    res.Body = "Hello, World!";
});

// Register POST route
server.Router.Post("/api/users", (req, res) =>
{
    res.StatusLine.StatusCode = "201";
    res.Headers["Location"] = "/api/users/123";
    res.Body = "User created";
});

// Register ANY (all methods) route
server.Router.Any("/health", (req, res) =>
{
    res.StatusLine.StatusCode = "200";
    res.Body = "OK";
});

// Start server
await server.Start(IPAddress.Parse("127.0.0.1"), 8000);
```

## Testing

Comprehensive test suite with 67+ tests:
- **TestStreamParser** (11 tests): Low-level stream reading and line parsing
- **TestHttpResponse** (25 tests): Response formatting, headers, body, Content-Length
- **TestRouter** (25 tests): Route registration, lookup, handler execution, priority
- **Other tests**: Factory, request parsing, header validation

Run tests:
```bash
dotnet test
```

Generate coverage report:
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## HTTP Protocol Compliance

### RFC 9110 Features
- ✅ Strict header key validation (token format)
- ✅ Optional whitespace (OWS) handling
- ✅ Multi-value header support (comma-separated)
- ✅ Case-insensitive headers
- ✅ UTF-8 body support with automatic byte counting
- ✅ Standard HTTP response format with CRLF line endings

### Parsing Rules
```
REQUEST_LINE: METHOD SP REQUEST-TARGET SP HTTP-VERSION CRLF
HEADERS: (HEADER-NAME ": " HEADER-VALUE CRLF)+
BODY: Any UTF-8 content
```

## Technology Stack

- **.NET 10.0**: Latest C# runtime with modern language features
- **C# 13**: Primary constructors, nullable reference types, implicit usings
- **Microsoft.Extensions.Logging**: Structured logging
- **System.Net.Sockets**: Low-level TCP networking
- **System.Text**: UTF-8 encoding for accurate byte counting
- **System.Threading.Channels**: Async communication patterns
- **MSTest**: Unit testing framework
- **Moq 4.20+**: Mocking library for tests

## Code Quality

- **67+ unit tests** covering all major components
- **RFC 9110 compliance** for HTTP protocol
- **SOLID principles**: Dependency Injection, Interface Segregation, Single Responsibility
- **Async/await patterns** for non-blocking I/O
- **Type-safe models** with no magic strings
- **GitHub Actions CI/CD** with automated test running and coverage reporting

## Design Decisions

1. **Nested Dictionary routing** instead of regex patterns for performance and simplicity
2. **Case-insensitive headers** using `StringComparer.OrdinalIgnoreCase` per HTTP spec
3. **CSV concatenation** for multi-value headers instead of arrays
4. **Auto Content-Length calculation** in response ToString() for convenience
5. **Separate test projects** (HttpServer.Tests) for better organization
6. **Logging via DI** for observability and testing flexibility

## Learning Outcomes

This implementation demonstrates:
- How HTTP servers parse and route requests
- Dependency injection patterns in .NET
- Factory and Router design patterns
- RFC compliance in protocol implementation
- Async/await for I/O operations
- Test-driven development with comprehensive coverage
- GitHub Actions CI/CD setup
- Clean code architecture principles

- Design patterns (Factory, Strategy via interfaces)
- Clean code organization and separation of concerns

## Building and Running

```bash
dotnet build
dotnet run
```

The server will start on `127.0.0.1:8000` and wait for incoming connections.

## Testing

You can test the server with curl:
```bash
curl -X GET http://localhost:8000/ -d "test data"
```
