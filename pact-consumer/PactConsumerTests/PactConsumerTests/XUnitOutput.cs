using PactNet.Infrastructure.Outputters;

using Xunit.Abstractions;

namespace PactConsumerTests
{
    public class XUnitOutput : IOutput
    {
        private readonly ITestOutputHelper output;
        public XUnitOutput(ITestOutputHelper output)
        {
            this.output = output;
        }
        public void WriteLine(string line)
        {
            output.WriteLine(line);
        }
    }
}
