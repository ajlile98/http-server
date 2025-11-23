# HTTP Server

A technical demonstration of design patterns and software architecture principles implemented in C# using .NET 9.

## Overview

This project implements a simple HTTP server from scratch, showcasing fundamental networking concepts, design patterns, and clean code architecture. It's designed as a learning tool to explore how HTTP servers process requests at a low level.

## Project Structure

```
http-server/
├── Models/                 # Data models
│   ├── HttpRequest.cs     # Represents HTTP request data
│   └── HttpResponse.cs    # Represents HTTP response data
├── Interfaces/            # Contract definitions
│   ├── IHttpServer.cs     # Server interface contract
│   └── IStreamParser.cs   # Parser interface contract
├── Factories/             # Object creation
│   └── RequestFactory.cs  # Creates request objects
├── Services/              # Core implementation
│   ├── HttpServer.cs      # Main server logic
│   ├── HttpBinaryReader.cs # Reads HTTP protocol from streams
│   └── ByteStreamParser.cs # Parses binary stream data
├── Program.cs             # Application entry point
└── http-server.csproj     # Project configuration
```

## Key Design Patterns

### 1. Dependency Injection
The `HttpServer` class accepts an optional `IStreamParser` interface, allowing for flexible parser implementations without tight coupling.

```csharp
public HttpServer(IStreamParser? parser = null)
{
    _parser = parser ?? new ByteStreamParser();
}
```

### 2. Factory Pattern
The `RequestFactory` encapsulates the logic for creating `HttpRequest` objects from parsed data, promoting reusability and separation of concerns.

### 3. Interface Segregation
Clean contracts defined through `IHttpServer` and `IStreamParser` allow for easy testing and alternative implementations.

### 4. Route Handler Pattern
Routes are registered as delegates, enabling flexible request handling:

```csharp
server.RegisterRoute("/", GetHttpHandler);
```

## Core Components

### HttpServer
Manages TCP connections, listens for incoming requests, and dispatches them to registered route handlers. Uses async/await for non-blocking I/O operations.

### ByteStreamParser & HttpBinaryReader
Handle low-level HTTP protocol parsing from raw TCP streams, converting binary data into structured request objects.

### Models
Type-safe representations of HTTP concepts:
- `HttpRequest`: Method, path, headers, body
- `HttpResponse`: Status code, status message, headers, body

## Usage

Create an HTTP handler function:
```csharp
void GetHttpHandler(HttpRequest req, HttpResponse res)
{
    res.StatusCode = 200;
    res.StatusMessage = "OK";
    res.Headers["Content-Type"] = "text/plain";
    res.Body = "Hello from the handler!";
}
```

Register it and start the server:
```csharp
HttpServer server = new HttpServer();
server.RegisterRoute("/", GetHttpHandler);
await server.Start(IPAddress.Parse("127.0.0.1"), 42069);
```

## Technology Stack

- **.NET 9.0**: Modern C# runtime
- **C# Language Features**: Records, nullable reference types, implicit usings
- **System.Net.Sockets**: Low-level networking

## Learning Objectives

This project demonstrates:
- HTTP protocol fundamentals
- Async/await patterns for I/O operations
- TCP socket programming
- Stream parsing and protocol implementation
- SOLID principles (Dependency Injection, Interface Segregation)
- Design patterns (Factory, Strategy via interfaces)
- Clean code organization and separation of concerns

## Building and Running

```bash
dotnet build
dotnet run
```

The server will start on `127.0.0.1:42069` and wait for incoming connections.

## Testing

You can test the server with curl:
```bash
curl -X GET http://localhost:42069/ -d "test data"
```
