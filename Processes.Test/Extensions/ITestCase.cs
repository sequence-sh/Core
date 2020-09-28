using Xunit.Abstractions;

namespace Reductech.EDR.Processes.Test.Extensions
{
    public interface ITestCase
    {
        string Name { get; }
        void Execute(ITestOutputHelper testOutputHelper);
    }
}