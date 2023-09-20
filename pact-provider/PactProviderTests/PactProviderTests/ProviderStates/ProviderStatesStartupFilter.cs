using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using PactProviderMockAPI.Services;

namespace PactProviderTests.ProviderStates
{
    public sealed class ProviderStatesStartupFilter : IStartupFilter
    {
        private readonly ICreateProductRepository _createProductRepository;

        /// <summary>
        /// Configure startup to use ProviderStateMiddleware required for using Provider States in pact verification
        /// </summary>
        /// <param name="providerStates"></param>
        public ProviderStatesStartupFilter(ICreateProductRepository createProductRepository)
        {
            _createProductRepository = createProductRepository;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<ProviderStateMiddleware>(_createProductRepository);
                next(builder);
            };
        }
    }
}