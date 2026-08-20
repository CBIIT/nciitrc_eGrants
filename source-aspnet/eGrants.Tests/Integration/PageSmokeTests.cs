using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

namespace eGrants.Tests.Integration
{
    /// <summary>
    /// Smoke tests that request every auto-discovered GET page in the application and assert
    /// that none of them return a server error (5xx). Routes are discovered from the running
    /// application's action descriptors so the coverage stays in sync as pages are added.
    /// </summary>
    public class PageSmokeTests : IClassFixture<SmokeTestWebApplicationFactory>
    {
        private readonly SmokeTestWebApplicationFactory _factory;
        private readonly ITestOutputHelper _output;

        public PageSmokeTests(SmokeTestWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        public static IEnumerable<object[]> DiscoverGetRoutes()
        {
            using var factory = new SmokeTestWebApplicationFactory();

            // Forces the host to build so the action descriptors are available.
            _ = factory.Server;

            var provider = factory.Services.GetRequiredService<IActionDescriptorCollectionProvider>();

            var urls = new SortedSet<string>();

            foreach (var action in provider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>())
            {
                // Only exercise HTTP GET actions.
                if (!IsHttpGet(action))
                {
                    continue;
                }

                // Skip actions that require route parameters (e.g. {id}); a smoke test cannot
                // supply meaningful values, and a null/absent id would exercise a different path.
                if (RequiresRouteParameters(action))
                {
                    continue;
                }

                var url = BuildUrl(action);
                if (url is not null)
                {
                    urls.Add(url);
                }
            }

            return urls.Select(u => new object[] { u });
        }

        [Theory]
        [MemberData(nameof(DiscoverGetRoutes))]
        public async Task Page_Does_Not_Return_ServerError(string url)
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            HttpResponseMessage response = await client.GetAsync(url);

            _output.WriteLine($"GET {url} -> {(int)response.StatusCode} {response.StatusCode}");

            Assert.True(
                (int)response.StatusCode < 500,
                $"GET {url} returned server error {(int)response.StatusCode} ({response.StatusCode}).");
        }

        private static bool IsHttpGet(ControllerActionDescriptor action)
        {
            var httpMethodMetadata = action.EndpointMetadata?
                .OfType<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()
                .FirstOrDefault();

            // No explicit HTTP method metadata means the action is reachable via GET.
            if (httpMethodMetadata is null || httpMethodMetadata.HttpMethods.Count == 0)
            {
                return true;
            }

            return httpMethodMetadata.HttpMethods.Contains(HttpMethods.Get);
        }

        private static bool RequiresRouteParameters(ControllerActionDescriptor action)
        {
            return action.Parameters
                .Any(p => p.BindingInfo?.BindingSource == Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Path);
        }

        private static string? BuildUrl(ControllerActionDescriptor action)
        {
            // Prefer an explicit attribute-route template when present.
            var template = action.AttributeRouteInfo?.Template;
            if (!string.IsNullOrEmpty(template))
            {
                // Skip attribute routes that still contain tokens we cannot fill.
                if (template.Contains('{'))
                {
                    return null;
                }

                return "/" + template.TrimStart('/');
            }

            var controller = action.ControllerName;
            var actionName = action.ActionName;

            if (string.IsNullOrEmpty(controller) || string.IsNullOrEmpty(actionName))
            {
                return null;
            }

            // Convention-based route: /{controller}/{action}
            return $"/{controller}/{actionName}";
        }
    }
}
