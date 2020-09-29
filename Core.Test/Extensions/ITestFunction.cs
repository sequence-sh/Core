using Xunit.Abstractions;

namespace Reductech.EDR.Core.Test.Extensions
{
    public interface ITestFunction
    {
        string Name { get; }
        void Execute(ITestOutputHelper testOutputHelper);
    }
}