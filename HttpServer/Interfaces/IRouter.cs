using HttpServerApp.Models;
using HttpMethod = HttpServerApp.Models.HttpMethod;

namespace HttpServerApp.Interfaces;

public interface IRouter
{
  public void AddRoute(HttpMethod httpMethod, string routePath, Action<HttpRequest, HttpResponse> handler);
  public void Any(string routePath, Action<HttpRequest, HttpResponse> handler);
  public void Get(string routePath, Action<HttpRequest, HttpResponse> handler);
  public void Post(string routePath, Action<HttpRequest, HttpResponse> handler);
  public void Put(string routePath, Action<HttpRequest, HttpResponse> handler);
  public void Delete(string routePath, Action<HttpRequest, HttpResponse> handler);
  public Route? FindRoute(string method, string path);
}
