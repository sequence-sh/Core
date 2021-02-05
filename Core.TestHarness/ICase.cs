using System;
using System.Collections.Generic;
using AutoTheory;
using CSharpFunctionalExtensions;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Thinktecture;

namespace Reductech.EDR.Core.TestHarness
{

/// <summary>
/// A case that executes a step.
/// </summary>
public interface ICaseThatExecutes : IAsyncTestInstance
{
    Dictionary<VariableName, object> ExpectedFinalState { get; }

    public bool IgnoreFinalState { get; set; }

    Maybe<StepFactoryStore> StepFactoryStoreToUse { get; set; }

    void AddExternalProcessRunnerAction(Action<Mock<IExternalProcessRunner>> action);
    void AddFileSystemAction(Action<Mock<IFileSystemHelper>> action);
    void AddConsoleAction(Action<Mock<IConsole>> action);

    SCLSettings Settings { get; set; }
}

}
