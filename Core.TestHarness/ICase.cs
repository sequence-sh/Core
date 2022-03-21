using Reductech.Sequence.Core.TestHarness.Rest;

namespace Reductech.Sequence.Core.TestHarness;

/// <summary>
/// A case that executes a step.
/// </summary>
public interface ICaseThatExecutes : IAsyncTestInstance, ICaseWithSetup
{
    Dictionary<VariableName, ISCLObject> ExpectedFinalState { get; }

    public bool IgnoreFinalState { get; set; }

    public List<Action<IExternalContext>> FinalContextChecks { get; }

    /// <summary>
    /// The step factory store to use for running the step.
    /// The default step factory store will still be used for deserialization.
    /// </summary>
    Maybe<StepFactoryStore> StepFactoryStoreToUse { get; set; }

    LogLevel CheckLogLevel { get; set; }
}

public interface ICaseWithSetup
{
    ExternalContextSetupHelper ExternalContextSetupHelper { get; }
    RESTClientSetupHelper RESTClientSetupHelper { get; }
    public List<Action> FinalChecks { get; }

    Dictionary<VariableName, ISCLObject> InjectedVariables { get; }
}
