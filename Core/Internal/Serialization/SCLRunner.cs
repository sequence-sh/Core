using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Internal.Errors;
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
        IExternalContext externalContext,
        params KeyValuePair<string, object>[] loggingData)
    {
        _settings         = settings;
        _logger           = logger;
        _stepFactoryStore = stepFactoryStore;
        _externalContext  = externalContext;

        _loggingData = loggingData.ToDictionary(x => x.Key, x => x.Value);
    }

    private readonly SCLSettings _settings;
    private readonly ILogger _logger;
    private readonly StepFactoryStore _stepFactoryStore;
    private readonly IExternalContext _externalContext;

    private readonly IReadOnlyDictionary<string, object> _loggingData;

    /// <summary>
    /// Run step defined in an SCL string.
    /// </summary>
    /// <param name="text">SCL representing the step.</param>
    /// <param name="cancellationToken">Cancellation ErrorLocation</param>
    /// <returns></returns>
    [UsedImplicitly]
    public async Task<Result<Unit, IError>> RunSequenceFromTextAsync(
        string text,
        CancellationToken cancellationToken)
    {
        var stepResult = SCLParsing.ParseSequence(text)
            .Bind(x => x.TryFreeze(_stepFactoryStore))
            .Map(ConvertToUnitStep);

        if (stepResult.IsFailure)
            return stepResult.ConvertFailure<Unit>();

        using var loggingScope = _logger.BeginScope(_loggingData);

        using var stateMonad = new StateMonad(
            _logger,
            _settings,
            _stepFactoryStore,
            _externalContext
        );

        var runResult = await stepResult.Value.Run(stateMonad, cancellationToken);

        return runResult;
    }

    /// <summary>
    /// Converts the step to a unit step for running.
    /// </summary>
    public static IStep<Unit> ConvertToUnitStep(IStep step)
    {
        if (step is IStep<Unit> unitStep)
            return unitStep;

        IStep<Unit> print = SurroundWithPrint(step as dynamic);
        return print;
    }

    private static IStep<Unit> SurroundWithPrint<T>(IStep<T> step)
    {
        var p = new Print<T>() { Value = step };

        return p;
    }

    /// <summary>
    /// Run step defined in an SCL file.
    /// </summary>
    /// <param name="path">Path to the SCL file.</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    [UsedImplicitly]
    public async Task<Result<Unit, IError>> RunSequenceFromPathAsync(
        string path,
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
            result = new SingleError(
                EntireSequenceLocation.Instance,
                e,
                ErrorCode.ExternalProcessError
            );
        }
        #pragma warning restore CA1031 // Do not catch general exception types

        var result2 = await result.Bind(x => RunSequenceFromTextAsync(x, cancellationToken));
        return result2;
    }
}

}
