
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpServer.Tests;

[TestClass]
public sealed class Test1
{

  [TestMethod]
  public void TestReadFile()
  {
    MemoryStream memoryStream = new();
    string message = "Hello World";
    var message_bytes = Encoding.UTF8.GetBytes(message);
    memoryStream.Write(message_bytes, 0, message_bytes.Length);
    memoryStream.Seek(0, SeekOrigin.Begin);
    var s = new StreamReader(memoryStream);

    var buffer = new char[8];
    int charsRead = s.ReadBlock(buffer, 0, buffer.Length);
    string result = new string(buffer, 0, charsRead);
    Assert.AreEqual("Hello Wo", result);  // This shows in output
  }
}
