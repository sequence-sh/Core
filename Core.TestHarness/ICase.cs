using Sequence.Core.TestHarness.Rest;

namespace Sequence.Core.TestHarness;

/// <summary>
/// A case that executes a step.
/// </summary>
public interface ICaseThatExecutes : IAsyncTestInstance, ICaseWithSetup
{
    /// <summary>
    /// The expected state after the test is run
    /// </summary>
    Dictionary<VariableName, ISCLObject> ExpectedFinalState { get; }

    /// <summary>
    /// Whether to ignore the state after the test has finished executing
    /// </summary>
    public bool IgnoreFinalState { get; set; }

    /// <summary>
    /// Checks 
    /// </summary>
    public List<Action<IExternalContext>> FinalContextChecks { get; }

    /// <summary>
    /// The step factory store to use for running the step.
    /// The default step factory store will still be used for deserialization.
    /// </summary>
    Maybe<StepFactoryStore> StepFactoryStoreToUse { get; set; }

    /// <summary>
    /// Only check log messages that are this level or higher
    /// </summary>
    LogLevel CheckLogLevel { get; set; }
}

/// <summary>
/// A test case that sets up an external context
/// </summary>
public interface ICaseWithSetup
{
    /// <summary>
    /// Sets up the external context
    /// </summary>
    ExternalContextSetupHelper ExternalContextSetupHelper { get; }

    /// <summary>
    /// Sets up the REST client
    /// </summary>
    RESTClientSetupHelper RESTClientSetupHelper { get; }

    /// <summary>
    /// Checks that are executed after the test
    /// </summary>
    public List<Action> FinalChecks { get; }

    /// <summary>
    /// Variables to inject
    /// </summary>
    Dictionary<VariableName, InjectedVariable> InjectedVariables { get; }
}
