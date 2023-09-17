using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace PactProviderTests.ProviderStates
{
    public sealed class ProviderStatesStartupFilter : IStartupFilter
    {
        private readonly ProviderStatesContainer _providerStates;

        /// <summary>
        /// Configure startup to use ProviderStateMiddleware required for using Provider States in pact verification
        /// </summary>
        /// <param name="providerStates"></param>
        public ProviderStatesStartupFilter(ProviderStatesContainer providerStates)
        {
            _providerStates = providerStates ?? throw new ArgumentNullException(nameof(providerStates));
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<ProviderStateMiddleware>(_providerStates);
                next(builder);
            };
        }
    }
}