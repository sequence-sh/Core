﻿namespace Sequence.Core.Steps;

/// <summary>
/// Writes to the console standard output
/// </summary>
[Alias("ToStandardOut")]
[Alias("WriteStandardOut")]
[Alias("ToStdOut")]
[Alias("WriteStdOut")]
[Alias("StdOutWrite")]
public class StandardOutWrite : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var data = await Data.Run(stateMonad, cancellationToken);

        if (data.IsFailure)
            return data.ConvertFailure<Unit>();

        var (stream, _) = data.Value.GetStream();

        try
        {
            await stream.CopyToAsync(
                stateMonad.ExternalContext.Console.OpenStandardOutput(),
                cancellationToken
            );
        }
        catch (Exception e)
        {
            return Result.Failure<Unit, IError>(
                ErrorCode.ExternalProcessError.ToErrorBuilder(e).WithLocation(this)
            );
        }

        return Unit.Default;
    }

    /// <summary>
    /// The data to write
    /// </summary>

    [StepProperty(1)]
    [Required]
    public IStep<StringStream> Data { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StandardOutWrite, Unit>();
}
