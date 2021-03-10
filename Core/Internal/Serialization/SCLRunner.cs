using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Logging;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal.Serialization
{

/// <summary>
/// Runs processes from Text
/// </summary>
public sealed class SCLRunner
{
    /// <summary>
    /// Creates a new SCL Runner
    /// </summary>
    public SCLRunner(
        SCLSettings settings,
        ILogger logger,
        StepFactoryStore stepFactoryStore,
        IExternalContext externalContext)
    {
        _settings         = settings;
        _logger           = logger;
        _stepFactoryStore = stepFactoryStore;
        _externalContext  = externalContext;
    }

    private readonly SCLSettings _settings;
    private readonly ILogger _logger;
    private readonly StepFactoryStore _stepFactoryStore;
    private readonly IExternalContext _externalContext;

    /// <summary>
    /// Run step defined in an SCL string.
    /// </summary>
    /// <param name="text">SCL representing the step.</param>
    /// <param name="sequenceMetadata">Additional information about the sequence</param>
    /// <param name="cancellationToken">Cancellation ErrorLocation</param>
    /// <returns></returns>
    [UsedImplicitly]
    public async Task<Result<Unit, IError>> RunSequenceFromTextAsync(
        string text,
        Dictionary<string, object> sequenceMetadata,
        CancellationToken cancellationToken)
    {
        sequenceMetadata[SCLTextName]    = text;
        sequenceMetadata[SequenceIdName] = Guid.NewGuid();
        return await RunSequence(text, sequenceMetadata, cancellationToken);
    }

    /// <summary>
    /// Run step defined in an SCL file.
    /// </summary>
    /// <param name="path">Path to the SCL file.</param>
    /// <param name="sequenceMetadata">Additional metadata about the sequence</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    [UsedImplicitly]
    public async Task<Result<Unit, IError>> RunSequenceFromPathAsync(
        string path,
        Dictionary<string, object> sequenceMetadata,
        CancellationToken cancellationToken)
    {
        Result<string, IError> result;

        try
        {
            result = await File.ReadAllTextAsync(path, cancellationToken);
        }
        #pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception e)
        {
            return ErrorCode.ExternalProcessError.ToErrorBuilder(e)
                .WithLocationSingle(ErrorLocation.EmptyLocation);
        }
        #pragma warning restore CA1031 // Do not catch general exception types

        sequenceMetadata[SCLPathName]    = path;
        sequenceMetadata[SequenceIdName] = Guid.NewGuid();

        var result2 = await result.Bind(x => RunSequence(x, sequenceMetadata, cancellationToken));

        return result2;
    }

    /// <summary>
    /// The name of the SequenceId log property.
    /// </summary>
    public const string SequenceIdName = "SequenceId";

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
    public const string TopLevelLoggingScope = "EDR";

    /// <summary>
    /// Runs an SCL sequence without injecting any metadata
    /// </summary>
    public async Task<Result<Unit, IError>> RunSequence(
        string text,
        IReadOnlyDictionary<string, object> sequenceMetadata,
        CancellationToken cancellationToken)
    {
        var stepResult = SCLParsing.ParseSequence(text)
            .Bind(x => x.TryFreeze(TypeReference.Any.Instance, _stepFactoryStore))
            .Map(ConvertToUnitStep);

        if (stepResult.IsFailure)
            return stepResult.ConvertFailure<Unit>();

        using var loggingScope = _logger.BeginScope(TopLevelLoggingScope);

        var stateMonad = new StateMonad(
            _logger,
            _settings,
            _stepFactoryStore,
            _externalContext,
            sequenceMetadata
        );

        LogSituation.SequenceStarted.Log(stateMonad, null);

        var connectorSettings = _settings.Entity.TryGetValue(SCLSettings.ConnectorsKey);

        if (connectorSettings.HasValue)
        {
            LogSituation.ConnectorSettings.Log(
                stateMonad,
                null,
                connectorSettings.Value.Serialize()
            );
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

    private static IStep<Unit> SurroundWithLog<T>(IStep<T> step)
    {
        var p = new Log<T> { Value = step };

        return p;
    }

    /// <summary>
    /// Logs an error
    /// </summary>
    public static void LogError(ILogger logger, IError error)
    {
        foreach (var singleError in error.GetAllErrors())
        {
            if (singleError.Exception != null)
                logger.LogError(
                    singleError.Exception,
                    "{Error} - {StepName} {Location}",
                    singleError.Message,
                    singleError.Location.StepName,
                    singleError.Location.TextLocation
                );
            else
                logger.LogError(
                    "{Error} - {StepName} {Location}",
                    singleError.Message,
                    singleError.Location.StepName,
                    singleError.Location.TextLocation
                );
        }
    }
}

}
