using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PactProviderMockAPI;
using PactProviderMockAPI.Models;
using PactProviderMockAPI.Services;
using PactProviderTests.ProviderStates;
using Xunit.Abstractions;

namespace PactProviderTests
{
    // [Collection("ProviderPact")]
    public class ProviderPactTests : HttpPactVerifier<Startup>
    {
        private string _providerUri { get; }
        private string _pactServiceUri { get; }
        private IWebHost _webHost { get; }
        private ITestOutputHelper _outputHelper { get; }
        private readonly ICreateProductRepository _mockCreateProductRepository;
        public ProviderPactTests(ITestOutputHelper output) : base(new PactVerifierOptions
        {
            ProviderName = ProviderPactTestsConstants.ProviderName,
            ConsumerName = ProviderPactTestsConstants.ConsumerName,
            PactPath = Path.Combine(
               @"./Pacts",
               "Order-Consumer-Order-Provider.json"),
            ProviderUri = "http://localhost:5000"
        }, output)
        {
            _mockCreateProductRepository = new InMemoryCreateProductRepository();

        }

        protected override void ConfigureTestServices(IServiceCollection services)
        {
            services.AddSingleton(_ => _mockCreateProductRepository);
            services.AddSingleton(_ => new ProviderStatesContainer(config =>
            {
                config.Add("", CreateProduct);
                config.Add("There is a new product", CreateProduct);
                config.Add("The product already exists", DuplicateProduct);
            }));
        }

        [Fact]
        public void EnsureCreateProduct()
        {
            VerifyAndPublishResults();
        }
        private void CreateProduct()
        {
            _mockCreateProductRepository.CreateProduct(new CreateProductRequest
            {
                ProductID = 123,
                ProductName = "Macbook air",
                ProductDescription = "Macbook air with M1 processor",
                Price = 2000
            });
        }

        private void DuplicateProduct()
        {
            _mockCreateProductRepository.DuplicateProduct();
        }
    }
}