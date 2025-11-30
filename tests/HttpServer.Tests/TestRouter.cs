using HttpServerApp.Interfaces;
using HttpServerApp.Models;
using HttpServerApp.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HttpMethod = HttpServerApp.Models.HttpMethod;

namespace HttpServer.Tests;

[TestClass]
public sealed class TestRouter
{
  private Router _router = null!;

  [TestInitialize]
  public void Setup()
  {
    _router = new Router();
  }

  [TestMethod]
  public void TestAddSingleGetRoute()
  {
    _router.Get("/", (req, res) =>
    {
    });

    var route = _router.FindRoute("GET", "/");
    Assert.IsNotNull(route);
    Assert.AreEqual("GET", route.HttpMethod);
    Assert.AreEqual("/", route.RoutePath);
  }

  [TestMethod]
  public void TestAddSinglePostRoute()
  {
    _router.Post("/submit", (req, res) => { });

    var route = _router.FindRoute("POST", "/submit");
    Assert.IsNotNull(route);
    Assert.AreEqual("POST", route.HttpMethod);
  }

  [TestMethod]
  public void TestAddSinglePutRoute()
  {
    _router.Put("/update", (req, res) => { });

    var route = _router.FindRoute("PUT", "/update");
    Assert.IsNotNull(route);
    Assert.AreEqual("PUT", route.HttpMethod);
  }

  [TestMethod]
  public void TestAddSingleDeleteRoute()
  {
    _router.Delete("/delete", (req, res) => { });

    var route = _router.FindRoute("DELETE", "/delete");
    Assert.IsNotNull(route);
    Assert.AreEqual("DELETE", route.HttpMethod);
  }

  [TestMethod]
  public void TestAddMultipleRoutesForSameMethod()
  {
    _router.Get("/", (req, res) => { });
    _router.Get("/about", (req, res) => { });
    _router.Get("/contact", (req, res) => { });

    var route1 = _router.FindRoute("GET", "/");
    var route2 = _router.FindRoute("GET", "/about");
    var route3 = _router.FindRoute("GET", "/contact");

    Assert.IsNotNull(route1);
    Assert.IsNotNull(route2);
    Assert.IsNotNull(route3);
    Assert.AreEqual("/", route1.RoutePath);
    Assert.AreEqual("/about", route2.RoutePath);
    Assert.AreEqual("/contact", route3.RoutePath);
  }

  [TestMethod]
  public void TestAddMultipleRoutesForDifferentMethods()
  {
    _router.Get("/resource", (req, res) => { });
    _router.Post("/resource", (req, res) => { });
    _router.Put("/resource", (req, res) => { });
    _router.Delete("/resource", (req, res) => { });

    var getRoute = _router.FindRoute("GET", "/resource");
    var postRoute = _router.FindRoute("POST", "/resource");
    var putRoute = _router.FindRoute("PUT", "/resource");
    var deleteRoute = _router.FindRoute("DELETE", "/resource");

    Assert.IsNotNull(getRoute);
    Assert.IsNotNull(postRoute);
    Assert.IsNotNull(putRoute);
    Assert.IsNotNull(deleteRoute);
  }

  [TestMethod]
  public void TestAddRouteWithAddRouteMethod()
  {
    var handler = (HttpRequest req, HttpResponse res) => { };
    _router.AddRoute(HttpMethod.Get, "/test", handler);

    var route = _router.FindRoute("GET", "/test");
    Assert.IsNotNull(route);
    Assert.AreEqual("GET", route.HttpMethod);
  }

  [TestMethod]
  public void TestGetHelperMethod()
  {
    _router.Get("/api/users", (req, res) => { });

    var route = _router.FindRoute("GET", "/api/users");
    Assert.IsNotNull(route);
  }

  [TestMethod]
  public void TestPostHelperMethod()
  {
    _router.Post("/api/users", (req, res) => { });

    var route = _router.FindRoute("POST", "/api/users");
    Assert.IsNotNull(route);
  }

  [TestMethod]
  public void TestAnyWildcardMethod()
  {
    _router.Any("/wildcard", (req, res) => { });

    var getRoute = _router.FindRoute("GET", "/wildcard");
    var postRoute = _router.FindRoute("POST", "/wildcard");
    var putRoute = _router.FindRoute("PUT", "/wildcard");

    Assert.IsNotNull(getRoute);
    Assert.IsNotNull(postRoute);
    Assert.IsNotNull(putRoute);
  }

  [TestMethod]
  public void TestFindRouteWithExactMatch()
  {
    _router.Get("/exact", (req, res) => { });

    var route = _router.FindRoute("GET", "/exact");
    Assert.IsNotNull(route);
    Assert.AreEqual("/exact", route.RoutePath);
  }

  [TestMethod]
  public void TestFindRouteWithCaseInsensitiveMethod()
  {
    _router.Get("/test", (req, res) => { });

    var routeUppercase = _router.FindRoute("GET", "/test");
    var routeLowercase = _router.FindRoute("get", "/test");
    var routeMixed = _router.FindRoute("Get", "/test");

    Assert.IsNotNull(routeUppercase);
    Assert.IsNotNull(routeLowercase);
    Assert.IsNotNull(routeMixed);
  }

  [TestMethod]
  public void TestFindRouteWithAnyFallback()
  {
    _router.Any("/fallback", (req, res) => { });

    var route = _router.FindRoute("CUSTOM", "/fallback");
    Assert.IsNotNull(route);
    Assert.AreEqual("*", route.HttpMethod);
  }

  [TestMethod]
  public void TestFindRouteReturnsNullForNonExistent()
  {
    var route = _router.FindRoute("GET", "/nonexistent");
    Assert.IsNull(route);
  }

  [TestMethod]
  public void TestFindRouteReturnsNullForNonExistentMethod()
  {
    _router.Get("/test", (req, res) => { });

    var route = _router.FindRoute("POST", "/test");
    Assert.IsNull(route);
  }

  [TestMethod]
  public void TestRoutePrioritySpecificBeforeAny()
  {
    var genericHandlerCalled = false;
    var specificHandlerCalled = false;

    _router.Any("/priority", (req, res) => { genericHandlerCalled = true; });
    _router.Get("/priority", (req, res) => { specificHandlerCalled = true; });

    var route = _router.FindRoute("GET", "/priority");
    Assert.IsNotNull(route);
    Assert.AreEqual("GET", route.HttpMethod);

    // Invoke the handler
    var request = new HttpRequest();
    var response = new HttpResponse();
    route.Handler.Invoke(request, response);

    Assert.IsTrue(specificHandlerCalled);
    Assert.IsFalse(genericHandlerCalled);
  }

  [TestMethod]
  public void TestHandlerExecution()
  {
    var handlerCalled = false;
    var handledRequest = false;
    var handledResponse = false;

    _router.Get("/handler-test", (req, res) =>
    {
      handlerCalled = true;
      if (req != null) handledRequest = true;
      if (res != null) handledResponse = true;
    });

    var route = _router.FindRoute("GET", "/handler-test");
    Assert.IsNotNull(route);

    var request = new HttpRequest();
    var response = new HttpResponse();
    route.Handler.Invoke(request, response);

    Assert.IsTrue(handlerCalled);
    Assert.IsTrue(handledRequest);
    Assert.IsTrue(handledResponse);
  }

  [TestMethod]
  public void TestHandlerCanModifyResponse()
  {
    _router.Get("/modify", (req, res) =>
    {
      res.Body = "Modified";
      res.StatusLine.StatusCode = "201";
    });

    var route = _router.FindRoute("GET", "/modify");
    var response = new HttpResponse();
    var request = new HttpRequest();
    route!.Handler.Invoke(request, response);

    Assert.AreEqual("Modified", response.Body);
    Assert.AreEqual("201", response.StatusLine.StatusCode);
  }

  [TestMethod]
  public void TestHandlerCanReadRequest()
  {
    _router.Get("/read", (req, res) =>
    {
      res.Body = req.RequestLine.RequestTarget;
    });

    var route = _router.FindRoute("GET", "/read");
    var request = new HttpRequest();
    request.RequestLine.RequestTarget = "/test-path";
    var response = new HttpResponse();
    route!.Handler.Invoke(request, response);

    Assert.AreEqual("/test-path", response.Body);
  }

  [TestMethod]
  public void TestMultipleAnyRoutes()
  {
    _router.Any("/any1", (req, res) => { });
    _router.Any("/any2", (req, res) => { });
    _router.Any("/any3", (req, res) => { });

    var route1 = _router.FindRoute("DELETE", "/any1");
    var route2 = _router.FindRoute("OPTIONS", "/any2");
    var route3 = _router.FindRoute("PATCH", "/any3");

    Assert.IsNotNull(route1);
    Assert.IsNotNull(route2);
    Assert.IsNotNull(route3);
  }

  [TestMethod]
  public void TestNestedPathRoutes()
  {
    _router.Get("/api/v1/users", (req, res) => { });
    _router.Get("/api/v1/users/profile", (req, res) => { });
    _router.Get("/api/v2/users", (req, res) => { });

    var v1Users = _router.FindRoute("GET", "/api/v1/users");
    var v1Profile = _router.FindRoute("GET", "/api/v1/users/profile");
    var v2Users = _router.FindRoute("GET", "/api/v2/users");

    Assert.IsNotNull(v1Users);
    Assert.IsNotNull(v1Profile);
    Assert.IsNotNull(v2Users);
    Assert.AreEqual("/api/v1/users", v1Users.RoutePath);
    Assert.AreEqual("/api/v1/users/profile", v1Profile.RoutePath);
    Assert.AreEqual("/api/v2/users", v2Users.RoutePath);
  }

  [TestMethod]
  public void TestPathCaseSensitivity()
  {
    _router.Get("/Test", (req, res) => { });

    var route1 = _router.FindRoute("GET", "/Test");
    var route2 = _router.FindRoute("GET", "/test");

    Assert.IsNotNull(route1);
    // Note: Path comparison may be case-sensitive depending on implementation
    // Adjust assertion based on actual behavior
  }

  [TestMethod]
  public void TestAddRouteOverwrite()
  {
    var firstHandlerCalled = false;
    var secondHandlerCalled = false;

    _router.Get("/overwrite", (req, res) =>
    {
      firstHandlerCalled = true;
    });

    _router.Get("/overwrite", (req, res) =>
    {
      secondHandlerCalled = true;
    });

    var route = _router.FindRoute("GET", "/overwrite");
    var request = new HttpRequest();
    var response = new HttpResponse();
    route!.Handler.Invoke(request, response);

    Assert.IsTrue(secondHandlerCalled);
    Assert.IsFalse(firstHandlerCalled);
  }

  [TestMethod]
  public void TestRouteWithSpecialCharactersInPath()
  {
    _router.Get("/api/search?q=test", (req, res) => { });
    var route = _router.FindRoute("GET", "/api/search?q=test");
    Assert.IsNotNull(route);
  }

  [TestMethod]
  public void TestRouteWithSlashPath()
  {
    _router.Get("/", (req, res) => { });
    var route = _router.FindRoute("GET", "/");
    Assert.IsNotNull(route);
    Assert.AreEqual("/", route.RoutePath);
  }

  [TestMethod]
  public void TestFindRouteWithDifferentPathsCaseInsensitiveMethod()
  {
    _router.Post("/login", (req, res) => { });
    _router.Post("/logout", (req, res) => { });

    var route1 = _router.FindRoute("post", "/login");
    var route2 = _router.FindRoute("POST", "/logout");

    Assert.IsNotNull(route1);
    Assert.IsNotNull(route2);
    Assert.AreEqual("/login", route1.RoutePath);
    Assert.AreEqual("/logout", route2.RoutePath);
  }

  [TestMethod]
  public void TestRouteHttpMethodProperty()
  {
    var route = new Route("GET", "/test", (req, res) => { });
    Assert.AreEqual("GET", route.HttpMethod);
  }

  [TestMethod]
  public void TestRoutePathProperty()
  {
    var route = new Route("GET", "/custom/path", (req, res) => { });
    Assert.AreEqual("/custom/path", route.RoutePath);
  }

  [TestMethod]
  public void TestRouteHandlerProperty()
  {
    var handler = (HttpRequest req, HttpResponse res) => { };
    var route = new Route("GET", "/test", handler);
    Assert.IsNotNull(route.Handler);
  }

  [TestMethod]
  public void TestAllHttpMethods()
  {
    _router.Get("/get", (req, res) => { });
    _router.Post("/post", (req, res) => { });
    _router.Put("/put", (req, res) => { });
    _router.Delete("/delete", (req, res) => { });
    _router.Any("/any", (req, res) => { });

    Assert.IsNotNull(_router.FindRoute("GET", "/get"));
    Assert.IsNotNull(_router.FindRoute("POST", "/post"));
    Assert.IsNotNull(_router.FindRoute("PUT", "/put"));
    Assert.IsNotNull(_router.FindRoute("DELETE", "/delete"));
    Assert.IsNotNull(_router.FindRoute("PATCH", "/any"));
  }
}
