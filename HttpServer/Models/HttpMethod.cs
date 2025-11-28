using System;

namespace HttpServerApp.Models;

public class HttpMethod(string method) : IEquatable<HttpMethod>
{
  public readonly string Method = method;

  public static HttpMethod Get { get; } = new("GET");
  public static HttpMethod Put { get; } = new("PUT");
  public static HttpMethod Post { get; } = new("POST");
  public static HttpMethod Delete { get; } = new("DELETE");
  public static HttpMethod Head { get; } = new("HEAD");
  public static HttpMethod Options { get; } = new("OPTIONS");
  public static HttpMethod Trace { get; } = new("TRACE");
  public static HttpMethod Patch { get; } = new("PATCH");

  public bool Equals(HttpMethod? other)
  {
    if (other == null)
      return false;
    return string.Equals(Method, other.Method, StringComparison.OrdinalIgnoreCase);
  }

  public override bool Equals(object? obj)
  {
    return Equals(obj as HttpMethod);
  }

  public override int GetHashCode()
  {
    return Method.ToUpperInvariant().GetHashCode();
  }

  public override string ToString()
  {
    return Method;
  }
}
