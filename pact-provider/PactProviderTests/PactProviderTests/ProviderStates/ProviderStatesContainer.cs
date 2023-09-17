namespace PactProviderTests.ProviderStates
{
    public sealed class ProviderStatesContainer
    {
        private readonly IDictionary<string, Action> _providerStates = new Dictionary<string, Action>();

        /// <summary>
        /// Create ProviderStates container for list of Provider States to be used for pact verification
        /// </summary>
        /// <param name="builder"></param>
        public ProviderStatesContainer(Action<IDictionary<string, Action>> builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Invoke(_providerStates);

            if (!_providerStates.ContainsKey(string.Empty))
            {
                throw new ArgumentException("A default state must be registered with key: \"\"", nameof(builder));
            }
        }

        internal bool TryGetState(string key, out Action setState)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _providerStates.TryGetValue(key, out setState);
        }
    }
}