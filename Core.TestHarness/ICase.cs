using System.Collections.Generic;
using System.Threading.Tasks;
using Reductech.EDR.Core.Internal;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.TestHarness
{
    public interface ICase
    {
        string Name { get; }

        public Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument);
    }

    public interface ICaseWithState : ICase
    {
        Dictionary<VariableName, object> InitialState { get; }
        Dictionary<VariableName, object> ExpectedFinalState { get; }

    }


}