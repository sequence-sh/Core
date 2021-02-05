using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Logging;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Deletes a file or folder from the file system.
/// </summary>
[Alias("Delete")]
public class DeleteItem : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var pathResult = await Path.Run(stateMonad, cancellationToken);

        if (pathResult.IsFailure)
            return pathResult.ConvertFailure<Unit>();

        var path = await pathResult.Value.GetStringAsync();

        Result<Unit, IErrorBuilder> result;

        if (stateMonad.ExternalContext.FileSystemHelper.Directory.Exists(path))
        {
            result = stateMonad.ExternalContext.FileSystemHelper.DeleteDirectory(path, true);
            stateMonad.Logger.LogSituation(LogSituation.DirectoryDeleted, new[] { path });
        }
        else if (stateMonad.ExternalContext.FileSystemHelper.File.Exists(path))
        {
            result = stateMonad.ExternalContext.FileSystemHelper.DeleteFile(path);
            stateMonad.Logger.LogSituation(LogSituation.FileDeleted, new[] { path });
        }
        else
        {
            result = Unit.Default;

            stateMonad.Logger.LogSituation(
                LogSituation.ItemToDeleteDidNotExist,
                new[] { path }
            );
        }

        return result.MapError(x => x.WithLocation(this));
    }

    /// <summary>
    /// The path to the file or folder to delete.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("File")]
    [Alias("Folder")]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => DeleteItemStepFactory.Instance;
}

/// <summary>
/// Deletes a file or folder from the file system.
/// </summary>
public class DeleteItemStepFactory : SimpleStepFactory<DeleteItem, Unit>
{
    private DeleteItemStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static SimpleStepFactory<DeleteItem, Unit> Instance { get; } =
        new DeleteItemStepFactory();
}

}
