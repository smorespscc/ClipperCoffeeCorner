using ClipperCoffeeCorner.Controllers;
using ClipperCoffeeCorner.Data;
using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Services;
using Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    [TestClass]
    public class PaymentTests
    {
        // Minimal IHttpClientFactory that returns the same HttpClient
        private class SimpleHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _client;
            public SimpleHttpClientFactory(HttpClient client) => _client = client;
            public HttpClient CreateClient(string name) => _client;
        }

        // Simple HttpMessageHandler to produce deterministic responses
        private class FixedResponseHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
            public FixedResponseHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_responder(request));
        }

        // Minimal in-memory ISession implementation suitable for unit tests
        private class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _storage = new();
            public IEnumerable<string> Keys => _storage.Keys;
            public string Id { get; } = Guid.NewGuid().ToString();
            public bool IsAvailable => true;

            public void Clear() => _storage.Clear();

            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public void Remove(string key) => _storage.Remove(key);

            public void Set(string key, byte[] value) => _storage[key] = value;

            public bool TryGetValue(string key, out byte[] value) => _storage.TryGetValue(key, out value);
        }

        // Fake IUrlHelper that returns a predictable action link
        private class FakeUrlHelper : IUrlHelper
        {
            private readonly string _actionUrl;
            public FakeUrlHelper(string actionUrl) => _actionUrl = actionUrl;

            public ActionContext ActionContext => throw new NotImplementedException();

            public string? Action(UrlActionContext actionContext) => _actionUrl;

            // Other members not needed by tests
            public string Content(string contentPath) => throw new NotImplementedException();
            public bool IsLocalUrl(string url) => throw new NotImplementedException();
            public string Link(string routeName, object values) => throw new NotImplementedException();
            public string RouteUrl(UrlRouteContext routeContext) => throw new NotImplementedException();
            public string? Action(string action, string controller, object? values, string protocol, string host, string fragment) => _actionUrl;
            public string? Action(string action, string controller, object? values) => _actionUrl;
            public string? Action(string action, string controller) => _actionUrl;
            public string? RouteUrl(string routeName, object? values, string? protocol, string? host, string? fragment) => throw new NotImplementedException();
            public string? RouteUrl(string routeName, object? values) => throw new NotImplementedException();
        }

        [TestMethod]
        public async Task SquareCheckoutService_ReturnsUrl_OnSuccessfulSquareResponse()
        {
            // Arrange: in-memory EF Core DB seeded with an order + item + combination
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"sq_{Guid.NewGuid()}")
                .Options;

            await using var db = new AppDbContext(options);

            var combo = new Combination
            {
                CombinationId = 100,
                Code = "C100",
                Price = 2.50m,
                IsActive = true,
                MenuItemId = 1
            };
            db.Combinations.Add(combo);

            var order = new Order
            {
                OrderId = 11,
                IdempotencyKey = Guid.NewGuid(),
                PlacedAt = DateTime.UtcNow,
                TotalAmount = 2.50m,
                Status = "Pending"
            };
            db.Orders.Add(order);

            db.OrderItems.Add(new OrderItem
            {
                OrderId = order.OrderId,
                CombinationId = combo.CombinationId,
                Quantity = 1,
                UnitPrice = combo.Price
            });

            await db.SaveChangesAsync();

            // Prepare fake Square HTTP response containing payment_link.url
            var payload = new { payment_link = new { url = "https://square.test/checkout/ok" } };
            var json = JsonSerializer.Serialize(payload);

            var handler = new FixedResponseHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://connect.squareupsandbox.com") };
            var factory = new SimpleHttpClientFactory(client);

            var inMemorySettings = new Dictionary<string, string>
            {
                ["Square:AccessToken"] = "fake-access",
                ["Square:LocationId"] = "loc-1",
                ["Square:ApiVersion"] = "2023-08-16"
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

            var svc = new SquareCheckoutService(factory, config, db);

            // Act
            var result = await svc.CreatePaymentLinkAsync(new Order { OrderId = order.OrderId }, "https://app/return");

            // Assert
            Assert.AreEqual("https://square.test/checkout/ok", result);
        }

        [TestMethod]
        public async Task SquareCheckoutService_Throws_WhenOrderMissing()
        {
            // Arrange empty DB
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"sq_missing_{Guid.NewGuid()}")
                .Options;
            await using var db = new AppDbContext(options);

            var handler = new FixedResponseHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://connect.squareupsandbox.com") };
            var factory = new SimpleHttpClientFactory(client);

            var inMemorySettings = new Dictionary<string, string>
            {
                ["Square:AccessToken"] = "fake-access",
                ["Square:LocationId"] = "loc-1"
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

            var svc = new SquareCheckoutService(factory, config, db);

            // Act / Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                await svc.CreatePaymentLinkAsync(new Order { OrderId = 9999 }));
        }

        [TestMethod]
        public async Task CheckoutController_CreateLink_RedirectsToSquareUrl()
        {
            // Arrange: mock square service that returns predictable link
            var expectedLink = "https://square.example/abc";
            var mockSquare = new TestSquareService(expectedLink);

            // DbContext not used by controller in this flow, but provide empty in-memory DB
            var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"co_{Guid.NewGuid()}")
                .Options;
            var db = new AppDbContext(dbOptions);

            var controller = new CheckoutController(mockSquare, db);

            // Provide HttpContext with session and request scheme
            var context = new DefaultHttpContext();
            var session = new TestSession();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = session });
            context.Request.Scheme = "https";
            controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Set Url helper so Url.Action doesn't throw; it should return a redirect URL that will be passed to service
            controller.Url = new FakeUrlHelper("https://app/checkout/success");

            // Put a CurrentOrder in session so controller will pass it through to the service
            var order = new Order { OrderId = 42, IdempotencyKey = Guid.NewGuid() };
            var jsonOrder = JsonSerializer.Serialize(order);
            // use session Set (ISession.Set expects bytes)
            session.Set("CurrentOrder", Encoding.UTF8.GetBytes(jsonOrder));

            // Act
            var result = await controller.CreateLink();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectResult));
            var redirect = (RedirectResult)result;
            Assert.AreEqual(expectedLink, redirect.Url);
        }

        [TestMethod]
        public void CheckoutController_Success_RemovesSessionAndRedirectsHome()
        {
            // Arrange
            var mockSquare = new TestSquareService("https://unused");
            var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"co2_{Guid.NewGuid()}")
                .Options;
            var db = new AppDbContext(dbOptions);
            var controller = new CheckoutController(mockSquare, db);

            var context = new DefaultHttpContext();
            var session = new TestSession();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = session });
            controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Place CurrentOrder marker
            session.Set("CurrentOrder", Encoding.UTF8.GetBytes("dummy"));

            // Pre-assert session contains key
            Assert.IsTrue(session.TryGetValue("CurrentOrder", out _));

            // Act
            var result = controller.Success();

            // Assert session no longer has the key
            Assert.IsFalse(session.TryGetValue("CurrentOrder", out _));

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var r = (RedirectToActionResult)result;
            Assert.AreEqual("Index", r.ActionName);
            Assert.AreEqual("Home", r.ControllerName);
        }

        // Minimal implementation of ISessionFeature used above
        private class SessionFeature : ISessionFeature
        {
            public ISession? Session { get; set; }
        }

        // Test double for ISquareCheckoutService used by controller tests
        private class TestSquareService : ISquareCheckoutService
        {
            private readonly string _url;
            public TestSquareService(string url) => _url = url;
            public Task<string> CreatePaymentLinkAsync(Order? order, string? redirectUrl = null) => Task.FromResult(_url);
        }
    }
}