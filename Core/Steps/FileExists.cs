using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Returns whether a file on the file system exists.
/// </summary>
[Alias("DoesFileExist")]
public class FileExists : CompoundStep<bool>
{
    /// <summary>
    /// The path to the file to check.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("File")]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<bool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var pathResult = await Path.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (pathResult.IsFailure)
            return pathResult.ConvertFailure<bool>();

        var r = stateMonad.FileSystemHelper.DoesFileExist(pathResult.Value);
        return r;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => FileExistsStepFactory.Instance;
}

/// <summary>
/// Returns whether a file on the file system exists.
/// </summary>
public class FileExistsStepFactory : SimpleStepFactory<FileExists, bool>
{
    private FileExistsStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static SimpleStepFactory<FileExists, bool> Instance { get; } =
        new FileExistsStepFactory();
}

}
