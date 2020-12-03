using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.TestHarness
{
    public interface ICase
    {
        string Name { get; }

        public Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument);
    }


    /// <summary>
    /// A case that executes a step.
    /// </summary>
    public interface ICaseThatExecutes : ICase
    {
        Dictionary<VariableName, object> ExpectedFinalState { get; }

        public bool IgnoreFinalState { get; set; }

        Maybe<StepFactoryStore> StepFactoryStoreToUse { get; set; }

        void AddExternalProcessRunnerAction(Action<Mock<IExternalProcessRunner>> action);
        void AddFileSystemAction(Action<Mock<IFileSystemHelper>> action);

        ISettings Settings {get; set; }

    }


}