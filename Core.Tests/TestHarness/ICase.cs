using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.TestHarness
{
    public interface ICase
    {
        string Name { get; }

        public Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument);
    }
}