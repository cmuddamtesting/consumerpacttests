using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PactProviderMockAPI.Models;
using PactProviderMockAPI.Services;
using System.Net;
using System.Text;

namespace PactProviderTests.ProviderStates
{
    public class ProviderStateMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICreateProductRepository _createProductRepository;
        private readonly IDictionary<string, Action> _providerStates;

        /// <summary>
        /// Set up Provider States configured for pact verification
        /// </summary>
        /// <param name="next"></param>
        /// <param name="providerStates"></param>
        public ProviderStateMiddleware(RequestDelegate next, ICreateProductRepository createProductRepository)
        {
            _next = next;
            _createProductRepository = createProductRepository;

            _providerStates = new Dictionary<string, Action>
            {
                { "", CreateProduct },
                { "There is a new product", CreateProduct },
                { "The product already exists", DuplicateProduct }
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!IsProviderStateRequest(context))
            {
                await _next.Invoke(context);
                return;
            }

            var providerState = await ParseProviderState(context);

            if (providerState is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("Provider state missing or badly formed");
                return;
            }

            if (!_providerStates.TryGetValue(providerState.State, out var state))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync($"No matching provider state for consumer:{providerState.Consumer} and state:{providerState.State}");
                return;
            }

            state.Invoke();
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        private static bool IsProviderStateRequest(HttpContext context)
        {
            // not at the right path
            if (!context.Request.Path.Value.Equals("/provider-states", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            // not the right HTTP method
            if (!context.Request.Method.Equals(HttpMethod.Post.Method, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static async Task<ProviderState> ParseProviderState(HttpContext context)
        {
            if (context.Request.Body is null)
            {
                return null;
            }

            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
            {
                var jsonRequestBody = await reader.ReadToEndAsync();
                var providerState = JsonConvert.DeserializeObject<ProviderState>(jsonRequestBody);
                providerState.State = providerState.State ?? "";
                providerState.Consumer = providerState.Consumer ?? "";
                return providerState;
            }
        }

        private void CreateProduct()
        {
            _createProductRepository.CreateProduct(new CreateProductRequest
            {
                ProductID = 123,
                ProductName = "Macbook air",
                ProductDescription = "Macbook air with M1 processor",
                Price = 2000
            });
        }

        private void DuplicateProduct()
        {
            _createProductRepository.DuplicateProduct();
        }
    }
}