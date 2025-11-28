using System;
using HttpServerApp.Interfaces;
using HttpServerApp.Models;
using HttpMethod = HttpServerApp.Models.HttpMethod;

namespace HttpServerApp.Services;

public class Router : IRouter
{
  private const string ANY_METHOD = "*";
  Dictionary<string, Dictionary<string, Route>> _routes = new(StringComparer.OrdinalIgnoreCase);
  public void AddRoute(HttpMethod httpMethod, string routePath, Action<HttpRequest, HttpResponse> handler)
  {
    var method = httpMethod.Method.ToUpperInvariant();

    if (!_routes.ContainsKey(method))
      _routes[method] = new(StringComparer.OrdinalIgnoreCase);

    _routes[method][routePath] = new Route(httpMethod.ToString(), routePath, handler);
  }
  public void Any(string routePath, Action<HttpRequest, HttpResponse> handler)
  {
    if (!_routes.ContainsKey(ANY_METHOD))
      _routes[ANY_METHOD] = new(StringComparer.OrdinalIgnoreCase);

    _routes[ANY_METHOD][routePath] = new Route(ANY_METHOD, routePath, handler);
  }
  public void Get(string routePath, Action<HttpRequest, HttpResponse> handler)
    => AddRoute(HttpMethod.Get, routePath, handler);
  public void Post(string routePath, Action<HttpRequest, HttpResponse> handler)
    => AddRoute(HttpMethod.Post, routePath, handler);
  public void Put(string routePath, Action<HttpRequest, HttpResponse> handler)
    => AddRoute(HttpMethod.Post, routePath, handler);
  public void Delete(string routePath, Action<HttpRequest, HttpResponse> handler)
    => AddRoute(HttpMethod.Delete, routePath, handler);
  public Route? FindRoute(string method, string path)
  {
    var upperMethod = method.ToUpperInvariant();
    
    if (_routes.TryGetValue(upperMethod, out var methodRoutes) && methodRoutes.TryGetValue(path, out var route))
      return route;
    
    if (_routes.TryGetValue(ANY_METHOD, out var anyRoutes) && anyRoutes.TryGetValue(path, out var anyRoute))
      return anyRoute;
    
    return null;

  }
}
