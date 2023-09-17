namespace PactProviderTests.ProviderStates
{
    public sealed class PactVerifierOptions
    {
        public string ProviderName { get; set; }
        public string ProviderUri { get; set; }
        public string PactPath { get; set; }
        public string ConsumerName { get; set; }
    }
}