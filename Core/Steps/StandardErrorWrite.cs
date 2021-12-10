namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Writes to the console standard error
/// </summary>
[SCLExample(
    "StandardErrorWrite 'Something Went Wrong'",
    Description = "Writes to the Standard Error",
    ExecuteInTests = false
)]
[Alias("ToStandardError")]
[Alias("WriteStandardError")]
[Alias("ToStdErr")]
[Alias("WriteStdErr")]
[Alias("StdErrWrite")]
public class StandardErrorWrite : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
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
                stateMonad.ExternalContext.Console.OpenStandardError(),
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
        new SimpleStepFactory<StandardErrorWrite, Unit>();
}
