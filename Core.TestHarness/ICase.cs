using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Reductech.EDR.Core.Internal;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.TestHarness
{
    public interface ICase
    {
        string Name { get; }

        public Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument);
    }

    public interface ICaseThatRuns : ICase
    {
        Dictionary<VariableName, object> InitialState { get; }
        Dictionary<VariableName, object> ExpectedFinalState { get; }

        void AddExternalProcessRunnerAction(Action<Mock<IExternalProcessRunner>> action);

        ISettings Settings {get; set; }

    }


}