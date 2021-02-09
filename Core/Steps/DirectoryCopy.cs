﻿using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Copy a directory
/// </summary>
public class DirectoryCopy : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var source = await SourceDirectory.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (source.IsFailure)
            return source.ConvertFailure<Unit>();

        var destination = await DestinationDirectory.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (destination.IsFailure)
            return destination.ConvertFailure<Unit>();

        var overwrite = await Overwrite.Run(stateMonad, cancellationToken);

        if (overwrite.IsFailure)
            return overwrite.ConvertFailure<Unit>();

        var copySubDirectories = await CopySubDirectories.Run(stateMonad, cancellationToken);

        if (copySubDirectories.IsFailure)
            return copySubDirectories.ConvertFailure<Unit>();

        var copyResult =
            DoCopy(
                source.Value,
                destination.Value,
                copySubDirectories.Value,
                overwrite.Value,
                stateMonad.ExternalContext.FileSystemHelper
            );

        if (copyResult.IsFailure)
            return copyResult.MapError(x => x.WithLocation(this));

        return Unit.Default;
    }

    /// <summary>
    /// The source directory name
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> SourceDirectory { get; set; } = null!;

    /// <summary>
    /// The destination directory name
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<StringStream> DestinationDirectory { get; set; } = null!;

    /// <summary>
    /// True if the destination file can be overwritten
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("false")]
    public IStep<bool> Overwrite { get; set; } = new BoolConstant(false);

    /// <summary>
    /// True to also copy subdirectories
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("false")]
    public IStep<bool> CopySubDirectories { get; set; } = new BoolConstant(false);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<FileCopy, Unit>();

    private static Result<Unit, IErrorBuilder> DoCopy(
        string sourceDirName,
        string destDirName,
        bool copySubDirs,
        bool overwrite,
        IFileSystem fileSystem)
    {
        if (!fileSystem.Directory.Exists(sourceDirName))
            return ErrorCode.DirectoryNotFound.ToErrorBuilder(sourceDirName);

        var dirs = fileSystem.Directory.GetDirectories(sourceDirName);

        // If the destination directory doesn't exist, create it.
        fileSystem.Directory.CreateDirectory(destDirName);

        // Get the files in the directory and copy them to the new location.
        var files = fileSystem.Directory.GetFiles(sourceDirName);

        foreach (var file in files)
        {
            string newPath = Path.Combine(destDirName, Path.GetFileName(file));
            fileSystem.File.Copy(file, newPath, overwrite);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (var subDirectory in dirs)
            {
                string newPath = Path.Combine(
                    destDirName,
                    Path.GetDirectoryName(subDirectory) ?? ""
                );

                var r = DoCopy(
                    subDirectory,
                    newPath,
                    copySubDirs,
                    overwrite,
                    fileSystem
                );

                if (r.IsFailure)
                    return r;
            }
        }

        return Unit.Default;
    }
}

}
