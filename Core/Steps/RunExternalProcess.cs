using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.ExternalProcesses;

namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Runs an external executable program.
/// </summary>
public sealed class RunExternalProcess : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var pathResult = await Path.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (pathResult.IsFailure)
            return pathResult.ConvertFailure<Unit>();

        List<string> arguments;

        if (Arguments == null)
            arguments = new List<string>();
        else
        {
            var argsResult = await Arguments.Run(stateMonad, cancellationToken)
                .Bind(x => x.GetElementsAsync(cancellationToken));

            if (argsResult.IsFailure)
                return argsResult.ConvertFailure<Unit>();

            arguments = new List<string>();

            foreach (var stringStream in argsResult.Value)
                arguments.Add(await stringStream.GetStringAsync());
        }

        var encodingResult = await Encoding.Run(stateMonad, cancellationToken);

        if (encodingResult.IsFailure)
            return encodingResult.ConvertFailure<Unit>();

        var r = await
            stateMonad.ExternalContext.ExternalProcessRunner.RunExternalProcess(
                    pathResult.Value,
                    IgnoreNoneErrorHandler.Instance,
                    arguments,
                    new Dictionary<string, string>(), //TODO let user control this
                    encodingResult.Value.Convert(),
                    stateMonad,
                    this,
                    cancellationToken
                )
                .MapError(x => x.WithLocation(this));

        return r;
    }

    /// <summary>
    /// The path to the external process
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <summary>
    /// Arguments to the step.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("No arguments")]
    public IStep<Array<StringStream>>? Arguments { get; set; }

    /// <summary>
    /// Encoding to use for the process output.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("Default encoding")]
    public IStep<EncodingEnum> Encoding { get; set; } =
        new EnumConstant<EncodingEnum>(EncodingEnum.Default);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<RunExternalProcess, Unit>();
}
