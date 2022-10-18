using Microsoft.Extensions.Logging;
using Reductech.Sequence.Core.Abstractions;
using Reductech.Sequence.Core.Internal.Logging;
using Reductech.Sequence.Core.Internal.Parser;
using Reductech.Sequence.Core.Steps;

namespace Reductech.Sequence.Core.Internal.Serialization;

/// <summary>
/// Runs processes from Text
/// </summary>
public sealed class SCLRunner
{
    /// <summary>
    /// Creates a new SCL Runner
    /// </summary>
    public SCLRunner(
        ILogger logger,
        StepFactoryStore stepFactoryStore,
        IExternalContext externalContext)
    {
        _logger           = logger;
        _stepFactoryStore = stepFactoryStore;
        _externalContext  = externalContext;
    }

    private readonly ILogger _logger;
    private readonly StepFactoryStore _stepFactoryStore;
    private readonly IExternalContext _externalContext;

    /// <summary>
    /// Run step defined in an SCL string.
    /// </summary>
    /// <param name="text">SCL representing the step.</param>
    /// <param name="sequenceMetadata">Additional information about the sequence</param>
    /// <param name="variablesToInject">Additional Variables to inject</param>
    /// <param name="cancellationToken">Cancellation ErrorLocation</param>
    /// <returns></returns>
    public async Task<Result<Unit, IError>> RunSequenceFromTextAsync(
        string text,
        Dictionary<string, object> sequenceMetadata,
        CancellationToken cancellationToken,
        IReadOnlyDictionary<VariableName, InjectedVariable>? variablesToInject = null)
    {
        sequenceMetadata[SCLTextName] = text;
        sequenceMetadata[RunIdName]   = Guid.NewGuid();
        return await RunSequence(text, sequenceMetadata, cancellationToken, variablesToInject);
    }

    /// <summary>
    /// Whether to Optimize the SCL that is run
    /// </summary>
    public bool OptimizeSCL { get; set; } = true;

    /// <summary>
    /// The name of the RunId log property.
    /// </summary>
    public const string RunIdName = "RunId";

    /// <summary>
    /// The name of the SCL Path log property
    /// </summary>
    public const string SCLPathName = "SCLPath";

    /// <summary>
    /// The name of the SCL Text log property
    /// </summary>
    public const string SCLTextName = "SCLText";

    /// <summary>
    /// The top level logging scope.
    /// </summary>
    public const string TopLevelLoggingScope = "Sequence";

    /// <summary>
    /// Caller metadata for the entire sequence.
    /// </summary>
    public static readonly CallerMetadata RootCallerMetadata = new(
        "Sequence",
        "Root",
        TypeReference.Any.Instance
    );

    /// <summary>
    /// Runs an SCL sequence
    /// </summary>
    public async Task<Result<Unit, IError>> RunSequence(
        string text,
        IReadOnlyDictionary<string, object> sequenceMetadata,
        CancellationToken cancellationToken,
        IReadOnlyDictionary<VariableName, InjectedVariable>? variablesToInject = null)
    {
        var settings = new OptimizationSettings(OptimizeSCL, OptimizeSCL, variablesToInject);

        var stepResult = SCLParsing.TryParseStep(text)
            .Bind(x => x.TryFreeze(RootCallerMetadata, _stepFactoryStore, settings))
            .Map(ConvertToUnitStep);

        if (stepResult.IsFailure)
            return stepResult.ConvertFailure<Unit>();

        using var loggingScope = _logger.BeginScope(TopLevelLoggingScope);

        var stateMonad = new StateMonad(
            _logger,
            _stepFactoryStore,
            _externalContext,
            sequenceMetadata
        );

        var initialVariables =
            (variablesToInject ?? ImmutableDictionary<VariableName, InjectedVariable>.Empty)
            .Select(x => new KeyValuePair<VariableName, ISCLObject>(x.Key, x.Value.SCLObject));

        var injectResult = await stateMonad.SetInitialVariablesAsync(initialVariables);

        if (injectResult.IsFailure)
            return injectResult.ConvertFailure<Unit>();

        LogSituation.SequenceStarted.Log(stateMonad, null);

        if (_stepFactoryStore.ConnectorData.Any())
        {
            if (LogSituation.ConnectorSettings.IsEnabled(stateMonad))
            {
                LogSituation.ConnectorSettings.Log(
                    stateMonad,
                    null,
                    stateMonad.Settings.Format()
                );
            }
        }

        var runResult = await stepResult.Value.Run(stateMonad, cancellationToken);

        await stateMonad.DisposeAsync();

        _logger.LogSituation(
            LogSituation.SequenceCompleted,
            null,
            sequenceMetadata,
            Array.Empty<object?>()
        );

        return runResult;
    }

    /// <summary>
    /// Converts the step to a unit step for running.
    /// </summary>
    public static IStep<Unit> ConvertToUnitStep(IStep step)
    {
        if (step is IStep<Unit> unitStep)
            return unitStep;

        IStep<Unit> log = SurroundWithLog(step as dynamic);
        return log;
    }

    private static IStep<Unit> SurroundWithLog<T>(IStep<T> step) where T : ISCLObject
    {
        var p = new Log { Value = step };

        return p;
    }
}
