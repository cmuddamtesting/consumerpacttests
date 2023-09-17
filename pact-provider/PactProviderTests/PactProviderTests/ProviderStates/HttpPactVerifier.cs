using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using PactNet.Verifier;
using Woolworths.Pact.Provider;
using Xunit.Abstractions;

namespace PactProviderTests.ProviderStates
{
    public abstract class HttpPactVerifier<TStartup> : IDisposable where TStartup : class
    {
        private readonly PactVerifier _verifier;
        private readonly string _pactBrokerUri;
        private readonly string _providerVersion;
        private readonly string _tag;
        private readonly PactVerifierOptions _options;
        public HttpPactVerifier(PactVerifierOptions options, ITestOutputHelper output)
        {
            _options = options;

            _verifier = new PactVerifier(new PactVerifierConfig
            {
                // NOTE: We default to using a ConsoleOutput,
                // however xUnit 2 does not capture the console output,
                // so a custom outputter is required.
                Outputters = new[]
                {
                    new XUnitOutput(output)
                }
            });

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PACTBROKER_PACT_URI")))
            {
                // check if triggered by pact webhook for contract content changed even, need to run verification only for changed contract
                // ensure the required env variables for pactUri and pactConsumer are passed in parameters of the webhook 
                _pactBrokerUri = Environment.GetEnvironmentVariable("PACTBROKER_PACT_URI");
                _providerVersion = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
                _tag = GetBranchName();
            }
        }

        public async void VerifyAndPublishResults()
        {
            // Act & Assert

            if (_pactBrokerUri == null)
            {
                await RunAsync(async () =>
                {
                    _verifier
                    .ServiceProvider(_options.ProviderName, new Uri(_options.ProviderUri))
                    .WithFileSource(new FileInfo(_options.PactPath))
                    .WithProviderStateUrl(new Uri(_options.ProviderUri + "/provider-states"))
                    .Verify();
                });
            }
            else
            {
                await RunAsync(async () =>
                {
                    _verifier
                    .ServiceProvider(_options.ProviderName, new Uri(_options.ProviderUri))
                    .WithPactBrokerSource(new Uri(_pactBrokerUri), (options) =>
                    {
                        options.ConsumerVersionSelectors(new ConsumerVersionSelector
                        {
                            Consumer = _options.ConsumerName,
                            Latest = true
                        }).PublishResults(_providerVersion, (configure) =>
                        {
                            configure.ProviderTags(_tag);
                        });
                    })
                    .WithProviderStateUrl(new Uri(_options.ProviderUri + "/provider-states"))
                    .Verify();
                });

            }
        }


        protected abstract void ConfigureTestServices(IServiceCollection services);

        private async Task RunAsync(Func<Task> action)
        {
            using (var host = BuildWebHost(_options.ProviderUri))
            {
                await host.StartAsync();

                await action.Invoke();

                await host.StopAsync();
            }
        }

        private IWebHost BuildWebHost(string url) => WebHost.CreateDefaultBuilder()
        .UseUrls(url)
        .UseStartup<TStartup>()
         .ConfigureTestServices(services =>
             {
                 ConfigureTestServices(services);
                 services.AddSingleton<IStartupFilter, ProviderStatesStartupFilter>();
             })
        .Build();
        /// <summary>
        /// Get the branch name if run from AzDevOps build. If not, returns empty string.
        /// </summary>
        /// <returns></returns>
        private string GetBranchName()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BUILD_SOURCEBRANCH")))
            {
                var branchName = Environment.GetEnvironmentVariable("BUILD_SOURCEBRANCH")?.Replace("refs/heads/", "").Replace('/', '_');
                return branchName;
            }
            return string.Empty;
        }
        public void Dispose()
        {
            _verifier.Dispose();
        }
    }
}