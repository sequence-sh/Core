using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Extensions
{
    public interface ITestFunction
    {
        string Name { get; }
        void Execute(ITestOutputHelper testOutputHelper);
    }
}