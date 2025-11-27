
using System.Text;
using HttpServerApp.Services;
using HttpServerApp.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace HttpServer.Tests;

[TestClass]
public sealed class TestStreamParser
{
  public TestContext? TestContext { get; set; }

  private readonly string message = """
  Do you have what it takes to be an engineer at TheStartup™?
  Are you willing to work 80 hours a week in hopes that your 0.001% equity is worth something?
  Can you say "synergy" and "democratize" with a straight face?
  Are you prepared to eat top ramen at your desk 3 meals a day?
  end
  """;

  private ILogger<T> CreateMockLogger<T>()
  {
    return new Mock<ILogger<T>>().Object;
  }

  [TestMethod]
  public void TestReadFile()
  {
    var message_bytes = Encoding.UTF8.GetBytes(message);
    MemoryStream memoryStream = new(message_bytes);

    var streamParser = new StreamParser(memoryStream, CreateMockLogger<StreamParser>());
    var line = streamParser.ParseLine();
    Assert.AreEqual("Do you have what it takes to be an engineer at TheStartup™?", line);  // This shows in output
  }
  
  [TestMethod]
  public async Task TestReadChannel()
  {
    var message_bytes = Encoding.UTF8.GetBytes(message);
    MemoryStream memoryStream = new(message_bytes);

    var streamParser = new StreamParser(memoryStream, CreateMockLogger<StreamParser>());
    var channel = streamParser.GetLinesChannel();

    var lines = new List<string>();
    await foreach (var line in channel.Reader.ReadAllAsync())
    {
      lines.Add(line);
    }

    Assert.HasCount(6, lines);
    Assert.AreEqual("Do you have what it takes to be an engineer at TheStartup™?", lines[0]);
    Assert.AreEqual("end", lines[4]);
  }

  [TestMethod]
  public async Task TestGoodGetRequestLine()
  {
    var requestText = "GET / HTTP/1.1\r\nHost: localhost:42069\r\nUser-Agent: curl/7.81.0\r\nAccept: */*\r\n\r\n";
    var requestBytes = Encoding.UTF8.GetBytes(requestText);
    var memoryStream = new MemoryStream(requestBytes);
    
    var logger = CreateMockLogger<StreamParser>();
    var parserFactory = new StreamParserFactory(logger);
    var httpFactory = new HttpFactory(CreateMockLogger<HttpFactory>(), parserFactory);
    
    var request = await httpFactory.GetRequestFromStream(memoryStream);

    Assert.IsNotNull(request);
    Assert.AreEqual("GET", request.RequestLine.Method);
    Assert.AreEqual("/", request.RequestLine.RequestTarget);
    Assert.AreEqual("1.1", request.RequestLine.HttpVersion);
  }

  [TestMethod]
  public async Task TestGoodGetRequestLineWithPath()
  {
    var requestText = "GET /coffee HTTP/1.1\r\nHost: localhost:42069\r\nUser-Agent: curl/7.81.0\r\nAccept: */*\r\n\r\n";
    var requestBytes = Encoding.UTF8.GetBytes(requestText);
    var memoryStream = new MemoryStream(requestBytes);
    
    var logger = CreateMockLogger<StreamParser>();
    var parserFactory = new StreamParserFactory(logger);
    var httpFactory = new HttpFactory(CreateMockLogger<HttpFactory>(), parserFactory);
    
    var request = await httpFactory.GetRequestFromStream(memoryStream);

    Assert.IsNotNull(request);
    Assert.AreEqual("GET", request.RequestLine.Method);
    Assert.AreEqual("/coffee", request.RequestLine.RequestTarget);
    Assert.AreEqual("1.1", request.RequestLine.HttpVersion);
  }

  [TestMethod]
  public async Task TestPostRequestWithBody()
  {
    var requestText = "POST /api/data HTTP/1.1\r\nHost: localhost:8000\r\nContent-Type: application/json\r\nContent-Length: 27\r\n\r\n{\"name\":\"test\",\"value\":123}";
    var requestBytes = Encoding.UTF8.GetBytes(requestText);
    var memoryStream = new MemoryStream(requestBytes);
    
    var logger = CreateMockLogger<StreamParser>();
    var parserFactory = new StreamParserFactory(logger);
    var httpFactory = new HttpFactory(CreateMockLogger<HttpFactory>(), parserFactory);
    
    var request = await httpFactory.GetRequestFromStream(memoryStream);

    TestContext?.WriteLine($"Full request:\n{request}");

    Assert.IsNotNull(request);
    Assert.AreEqual("POST", request.RequestLine.Method);
    Assert.AreEqual("/api/data", request.RequestLine.RequestTarget);
    Assert.AreEqual("1.1", request.RequestLine.HttpVersion);
    Assert.AreEqual("application/json", request.Headers["Content-Type"]);
    Assert.AreEqual("27", request.Headers["Content-Length"]);
    Assert.AreEqual("{\"name\":\"test\",\"value\":123}", request.Body);
  }

  [TestMethod]
  public async Task TestValidSingleHeader()
  {
    var requestText = "GET / HTTP/1.1\r\nHost: localhost:42069\r\n\r\n";
    var requestBytes = Encoding.UTF8.GetBytes(requestText);
    var memoryStream = new MemoryStream(requestBytes);
    
    var logger = CreateMockLogger<StreamParser>();
    var parserFactory = new StreamParserFactory(logger);
    var httpFactory = new HttpFactory(CreateMockLogger<HttpFactory>(), parserFactory);
    
    var request = await httpFactory.GetRequestFromStream(memoryStream);

    Assert.IsNotNull(request);
    Assert.IsTrue(request.Headers.ContainsKey("Host"));
    Assert.AreEqual("localhost:42069", request.Headers["Host"]);
  }

  [TestMethod]
  public async Task TestValidSingleHeaderWithExtraWhitespace()
  {
    var requestText = "GET / HTTP/1.1\r\n          Host: localhost:42069          \r\n\r\n";
    var requestBytes = Encoding.UTF8.GetBytes(requestText);
    var memoryStream = new MemoryStream(requestBytes);
    
    var logger = CreateMockLogger<StreamParser>();
    var parserFactory = new StreamParserFactory(logger);
    var httpFactory = new HttpFactory(CreateMockLogger<HttpFactory>(), parserFactory);
    
    var request = await httpFactory.GetRequestFromStream(memoryStream);

    Assert.IsNotNull(request);
    Assert.IsTrue(request.Headers.ContainsKey("Host"));
    Assert.AreEqual("localhost:42069", request.Headers["Host"]);
  }

  [TestMethod]
  public async Task TestValid2HeadersWithExistingHeaders()
  {
    var requestText = "GET / HTTP/1.1\r\nHost: localhost:42069\r\nUser-Agent: TestClient/1.0\r\nAccept: */*\r\n\r\n";
    var requestBytes = Encoding.UTF8.GetBytes(requestText);
    var memoryStream = new MemoryStream(requestBytes);
    
    var logger = CreateMockLogger<StreamParser>();
    var parserFactory = new StreamParserFactory(logger);
    var httpFactory = new HttpFactory(CreateMockLogger<HttpFactory>(), parserFactory);
    
    var request = await httpFactory.GetRequestFromStream(memoryStream);

    Assert.IsNotNull(request);
    Assert.HasCount(3, request.Headers);
    Assert.AreEqual("localhost:42069", request.Headers["Host"]);
    Assert.AreEqual("TestClient/1.0", request.Headers["User-Agent"]);
    Assert.AreEqual("*/*", request.Headers["Accept"]);
  }

  [TestMethod]
  public async Task TestInvalidSpacingHeader()
  {
    var requestText = "GET / HTTP/1.1\r\nHost : localhost:42069\r\n\r\n";
    var requestBytes = Encoding.UTF8.GetBytes(requestText);
    var memoryStream = new MemoryStream(requestBytes);
    
    var logger = CreateMockLogger<StreamParser>();
    var parserFactory = new StreamParserFactory(logger);
    var httpFactory = new HttpFactory(CreateMockLogger<HttpFactory>(), parserFactory);
    
    // Should throw FormatException for space before colon
    var exception = await Assert.ThrowsAsync<FormatException>(async () =>
    {
      await httpFactory.GetRequestFromStream(memoryStream);
    });
    
    Assert.IsNotNull(exception);
    Assert.Contains("space before colon", exception.Message);
  }

  [TestMethod]
  public async Task TestInvalidCharacterInHeaderKey()
  {
    var requestText = "GET / HTTP/1.1\r\nH©st: localhost:42069\r\n\r\n";
    var requestBytes = Encoding.UTF8.GetBytes(requestText);
    var memoryStream = new MemoryStream(requestBytes);
    
    var logger = CreateMockLogger<StreamParser>();
    var parserFactory = new StreamParserFactory(logger);
    var httpFactory = new HttpFactory(CreateMockLogger<HttpFactory>(), parserFactory);
    
    // Should throw FormatException for invalid character in header key
    var exception = await Assert.ThrowsAsync<FormatException>(async () =>
    {
      await httpFactory.GetRequestFromStream(memoryStream);
    });
    
    Assert.IsNotNull(exception);
  }

  [TestMethod]
  public async Task TestMultipleHeaderValuesAsCsv()
  {
    var requestText = "GET / HTTP/1.1\r\nAccept: text/html\r\nAccept: application/json\r\nAccept: */*\r\n\r\n";
    var requestBytes = Encoding.UTF8.GetBytes(requestText);
    var memoryStream = new MemoryStream(requestBytes);
    
    var logger = CreateMockLogger<StreamParser>();
    var parserFactory = new StreamParserFactory(logger);
    var httpFactory = new HttpFactory(CreateMockLogger<HttpFactory>(), parserFactory);
    
    var request = await httpFactory.GetRequestFromStream(memoryStream);

    TestContext?.WriteLine($"Accept header: {request.Headers["Accept"]}");

    Assert.IsNotNull(request);
    Assert.IsTrue(request.Headers.ContainsKey("Accept"));
    Assert.AreEqual("text/html, application/json, */*", request.Headers["Accept"]);
  }
}

