using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Core.TestHarness
{
    public interface ICase
    {
        string Name { get; }

        public Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument);
    }
}