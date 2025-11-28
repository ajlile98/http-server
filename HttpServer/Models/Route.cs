using System;

namespace HttpServerApp.Models;

public class Route(string httpMethod, string routePath, Action<HttpRequest, HttpResponse> handler)
{
  public string HttpMethod {get; set;} = httpMethod;
  public string RoutePath {get; set;} = routePath;
  public Action<HttpRequest, HttpResponse> Handler {get; set;} = handler;

}
