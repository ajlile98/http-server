using System.Text;
using HttpServerApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpServer.Tests;

[TestClass]
public sealed class TestHttpResponse
{
  [TestMethod]
  public void TestResponseWithNoBody()
  {
    var response = new HttpResponse();
    var result = response.ToString();

    Assert.IsTrue(result.Contains("HTTP/1.1 200 Ok"));
    Assert.IsTrue(result.Contains("Content-Length: 0"));
    // Empty line should exist between headers and body
    Assert.IsTrue(result.Contains("\n\n") || result.Contains("\r\n\r\n"));
  }

  [TestMethod]
  public void TestResponseWithSimpleBody()
  {
    var response = new HttpResponse
    {
      Body = "Hello, World!"
    };
    var result = response.ToString();

    Assert.IsTrue(result.Contains("HTTP/1.1 200 Ok"));
    Assert.IsTrue(result.Contains("Content-Length: 13"));
    Assert.IsTrue(result.Contains("Hello, World!"));
  }

  [TestMethod]
  public void TestResponseWithUtf8Characters()
  {
    var response = new HttpResponse
    {
      Body = "Hello, 世界! 🌍"
    };
    var result = response.ToString();

    var bodyBytes = Encoding.UTF8.GetByteCount("Hello, 世界! 🌍");
    Assert.IsTrue(result.Contains($"Content-Length: {bodyBytes}"));
    Assert.IsTrue(result.Contains("Hello, 世界! 🌍"));
  }

  [TestMethod]
  public void TestResponseWithMultipleHeaders()
  {
    var response = new HttpResponse
    {
      Body = "Test"
    };
    response.Headers["Content-Type"] = "text/plain";
    response.Headers["X-Custom-Header"] = "CustomValue";
    response.Headers["Cache-Control"] = "no-cache";

    var result = response.ToString();

    Assert.IsTrue(result.Contains("Content-Type: text/plain"));
    Assert.IsTrue(result.Contains("X-Custom-Header: CustomValue"));
    Assert.IsTrue(result.Contains("Cache-Control: no-cache"));
  }

  [TestMethod]
  public void TestResponseWith404StatusCode()
  {
    var response = new HttpResponse
    {
      Body = "Not Found"
    };
    response.StatusLine.StatusCode = "404";
    response.StatusLine.ReasonPhrase = "Not Found";

    var result = response.ToString();

    Assert.IsTrue(result.Contains("HTTP/1.1 404 Not Found"));
    Assert.IsTrue(result.Contains("Not Found"));
  }

  [TestMethod]
  public void TestResponseWith500StatusCode()
  {
    var response = new HttpResponse
    {
      Body = "Internal Server Error"
    };
    response.StatusLine.StatusCode = "500";
    response.StatusLine.ReasonPhrase = "Internal Server Error";

    var result = response.ToString();

    Assert.IsTrue(result.Contains("HTTP/1.1 500 Internal Server Error"));
  }

  [TestMethod]
  public void TestResponseWith400StatusCode()
  {
    var response = new HttpResponse
    {
      Body = "Bad Request"
    };
    response.StatusLine.StatusCode = "400";
    response.StatusLine.ReasonPhrase = "Bad Request";

    var result = response.ToString();

    Assert.IsTrue(result.Contains("HTTP/1.1 400 Bad Request"));
  }

  [TestMethod]
  public void TestResponseWithDifferentHttpVersion()
  {
    var response = new HttpResponse
    {
      Body = "Test"
    };
    response.StatusLine.HttpVersion = "HTTP/1.0";

    var result = response.ToString();

    Assert.IsTrue(result.Contains("HTTP/1.0 200 Ok"));
  }

  [TestMethod]
  public void TestResponseWithHttp2()
  {
    var response = new HttpResponse
    {
      Body = "Test"
    };
    response.StatusLine.HttpVersion = "HTTP/2";

    var result = response.ToString();

    Assert.IsTrue(result.Contains("HTTP/2 200 Ok"));
  }

  [TestMethod]
  public void TestEmptyLineBetweenHeadersAndBody()
  {
    var response = new HttpResponse
    {
      Body = "TestBody"
    };
    response.Headers["Content-Type"] = "text/plain";

    var result = response.ToString();

    // Verify empty line exists between headers and body
    // Split by newlines (handle both \n and \r\n)
    var lines = result.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    var emptyLineFound = false;
    for (int i = 0; i < lines.Length; i++)
    {
      if (string.IsNullOrEmpty(lines[i]))
      {
        emptyLineFound = true;
        break;
      }
    }
    Assert.IsTrue(emptyLineFound, "Empty line should exist between headers and body");
  }

  [TestMethod]
  public void TestCaseInsensitiveHeaders()
  {
    var response = new HttpResponse();
    response.Headers["content-type"] = "text/html";
    response.Headers["Content-Type"] = "application/json";

    // Should overwrite due to case-insensitive comparison
    Assert.AreEqual(1, response.Headers.Count);
    Assert.AreEqual("application/json", response.Headers["Content-Type"]);
  }

  [TestMethod]
  public void TestContentLengthUpdatesWithBodyChange()
  {
    var response = new HttpResponse
    {
      Body = "Short"
    };
    var result1 = response.ToString();
    Assert.IsTrue(result1.Contains("Content-Length: 5"));

    response.Body = "Much longer body content";
    var result2 = response.ToString();
    Assert.IsTrue(result2.Contains("Content-Length: 24"));
  }

  [TestMethod]
  public void TestJsonBody()
  {
    var jsonBody = """{"name": "John", "age": 30}""";
    var response = new HttpResponse
    {
      Body = jsonBody
    };
    response.Headers["Content-Type"] = "application/json";

    var result = response.ToString();

    Assert.IsTrue(result.Contains(jsonBody));
    Assert.IsTrue(result.Contains("Content-Type: application/json"));
    var expectedLength = Encoding.UTF8.GetByteCount(jsonBody);
    Assert.IsTrue(result.Contains($"Content-Length: {expectedLength}"));
  }

  [TestMethod]
  public void TestLargeBody()
  {
    var largeBody = new string('A', 10000);
    var response = new HttpResponse
    {
      Body = largeBody
    };

    var result = response.ToString();

    Assert.IsTrue(result.Contains(largeBody));
    Assert.IsTrue(result.Contains("Content-Length: 10000"));
  }

  [TestMethod]
  public void TestBodyWithNewlines()
  {
    var bodyWithNewlines = "Line 1\nLine 2\nLine 3";
    var response = new HttpResponse
    {
      Body = bodyWithNewlines
    };

    var result = response.ToString();

    Assert.IsTrue(result.Contains(bodyWithNewlines));
  }

  [TestMethod]
  public void TestBodyWithCRLF()
  {
    var bodyWithCRLF = "Line 1\r\nLine 2\r\nLine 3";
    var response = new HttpResponse
    {
      Body = bodyWithCRLF
    };

    var result = response.ToString();

    Assert.IsTrue(result.Contains(bodyWithCRLF));
  }

  [TestMethod]
  public void TestStatusLineDefaultValues()
  {
    var response = new HttpResponse();

    Assert.AreEqual("HTTP/1.1", response.StatusLine.HttpVersion);
    Assert.AreEqual("200", response.StatusLine.StatusCode);
    Assert.AreEqual("Ok", response.StatusLine.ReasonPhrase);
  }

  [TestMethod]
  public void TestStatusLineProperties()
  {
    var statusLine = new StatusLine
    {
      HttpVersion = "HTTP/1.0",
      StatusCode = "301",
      ReasonPhrase = "Moved Permanently"
    };

    Assert.AreEqual("HTTP/1.0", statusLine.HttpVersion);
    Assert.AreEqual("301", statusLine.StatusCode);
    Assert.AreEqual("Moved Permanently", statusLine.ReasonPhrase);
  }

  [TestMethod]
  public void TestBodyWithSpecialCharacters()
  {
    var bodyWithSpecial = "Test<>&\"'";
    var response = new HttpResponse
    {
      Body = bodyWithSpecial
    };

    var result = response.ToString();

    Assert.IsTrue(result.Contains(bodyWithSpecial));
  }

  [TestMethod]
  public void TestContentLengthWithMultibyteCharacters()
  {
    var multibyteBody = "日本語テスト";
    var response = new HttpResponse
    {
      Body = multibyteBody
    };

    var result = response.ToString();

    var expectedBytes = Encoding.UTF8.GetByteCount(multibyteBody);
    Assert.IsTrue(result.Contains($"Content-Length: {expectedBytes}"));
    // Japanese characters are 3 bytes each in UTF-8
    Assert.IsTrue(expectedBytes > 0);
  }

  [TestMethod]
  public void TestResponseFormatting()
  {
    var response = new HttpResponse
    {
      Body = "Hello"
    };
    response.Headers["Content-Type"] = "text/plain";

    var result = response.ToString();

    // Verify structure without being strict about line ending style
    Assert.IsTrue(result.Contains("HTTP/1.1 200 Ok"));
    Assert.IsTrue(result.Contains("Content-Type: text/plain"));
    Assert.IsTrue(result.Contains("Content-Length: 5"));
    Assert.IsTrue(result.EndsWith("Hello"));
  }

  [TestMethod]
  public void TestMultipleHeadersInResponse()
  {
    var response = new HttpResponse
    {
      Body = "Data"
    };
    response.Headers["Server"] = "MyServer/1.0";
    response.Headers["Date"] = "Mon, 28 Nov 2025 12:00:00 GMT";
    response.Headers["Connection"] = "keep-alive";

    var result = response.ToString();

    Assert.IsTrue(result.Contains("Server: MyServer/1.0"));
    Assert.IsTrue(result.Contains("Date: Mon, 28 Nov 2025 12:00:00 GMT"));
    Assert.IsTrue(result.Contains("Connection: keep-alive"));
  }

  [TestMethod]
  public void TestEmptyBodyStringIsEmpty()
  {
    var response = new HttpResponse();
    Assert.AreEqual("", response.Body);
  }

  [TestMethod]
  public void TestHeadersDictionaryExists()
  {
    var response = new HttpResponse();
    Assert.IsNotNull(response.Headers);
    Assert.IsInstanceOfType(response.Headers, typeof(Dictionary<string, string>));
  }

  [TestMethod]
  public void TestResponseWithAllCustomValues()
  {
    var response = new HttpResponse
    {
      Body = "Custom Response"
    };
    response.StatusLine.HttpVersion = "HTTP/1.0";
    response.StatusLine.StatusCode = "201";
    response.StatusLine.ReasonPhrase = "Created";
    response.Headers["Location"] = "/new-resource";
    response.Headers["X-Request-ID"] = "12345";

    var result = response.ToString();

    Assert.IsTrue(result.Contains("HTTP/1.0 201 Created"));
    Assert.IsTrue(result.Contains("Location: /new-resource"));
    Assert.IsTrue(result.Contains("X-Request-ID: 12345"));
    Assert.IsTrue(result.Contains("Custom Response"));
  }

  [TestMethod]
  public void TestContentLengthCalculationWithEmojis()
  {
    var emojiBody = "Hello 👋 World 🌍";
    var response = new HttpResponse
    {
      Body = emojiBody
    };

    var result = response.ToString();

    var expectedBytes = Encoding.UTF8.GetByteCount(emojiBody);
    Assert.IsTrue(result.Contains($"Content-Length: {expectedBytes}"));
  }
}
