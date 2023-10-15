using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PactConsumerTests.Models;
using PactNet;
using PactNet.Matchers;
using System.Net;
using Xunit.Abstractions;

namespace PactConsumerTests
{
    public class ConsumerTests
    {
        private readonly IPactBuilderV3 _pact;
        public ConsumerTests(ITestOutputHelper output)
        {
            var config = new PactConfig
            {
                // PactDir = @"./pacts",
                PactDir = Path.Join("..", "..", "..", "..", "pacts"),
                Outputters = new[]
    {
                    new XUnitOutput(output)
                },
                DefaultJsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };
            var pact = PactNet.Pact.V3(ConsumerPactTestsConstants.ConsumerName, ConsumerPactTestsConstants.ProviderName, config);
            _pact = pact.UsingNativeBackend();
        }
        [Fact]
        public async Task ValidateCreateProduct()
        {
            var createProductRequestString = File.ReadAllText(@"./Data/CreateProduct.json");
            var createProductRequest = JsonConvert.DeserializeObject<CreateProductRequest>(createProductRequestString);

            _pact
                .UponReceiving("A valid create product request")
                .Given("There is a new product")
                .WithRequest(HttpMethod.Post, "/v1/create")
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(createProductRequest)
                .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(
                    new
                    {
                        message = Match.Type("Product is created successfully"),
                        status = Match.Type("successful"),
                        status2 = Match.Type("successful"),
                        status3 = Match.Type("successful"),
                        status4 = Match.Type("successful"),
                        status5 = Match.Type("successful"),
                        status6 = Match.Type("successful")
                    }
                );

            await _pact.VerifyAsync(async ctx =>
            {
                var result = await CreateProductTestAsync(createProductRequest, ctx.MockServerUri);

                // Assert
                Assert.Equal("Product is created successfully", result.createProductResponse.Message);

            });
        }

        [Fact]
        public async Task ValidateDuplicateProduct()
        {
            var createProductRequestString = File.ReadAllText(@"./Data/CreateProduct.json");
            var createProductRequest = JsonConvert.DeserializeObject<CreateProductRequest>(createProductRequestString);

            _pact
                .UponReceiving("A valid create product request")
                .Given("The product already exists")
                .WithRequest(HttpMethod.Post, "/v1/create")
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(createProductRequest)
                .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(
                    new
                    {
                        message = Match.Type("The Product already exists")
                    }
                );

            await _pact.VerifyAsync(async ctx =>
            {
                var result = await CreateProductTestAsync(createProductRequest, ctx.MockServerUri);

                // Assert
                Assert.Equal("The Product already exists", result.createProductResponse.Message);

            });
        }

        public async Task<(CreateProductResponse createProductResponse, string error)> CreateProductTestAsync(CreateProductRequest request, Uri uri)
        {
            var sendRequest = new SendRequest(uri);
            var response = await sendRequest.SendRequestAsync<CreateProductRequest, CreateProductResponse>(request, "/v1/create");
            return response;
        }
    }
}