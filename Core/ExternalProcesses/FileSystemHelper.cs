//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using CSharpFunctionalExtensions;
//using Reductech.Sequence.Core.Internal.Errors;
//using Reductech.Sequence.Core.Util;

//namespace Reductech.Sequence.Core.ExternalProcesses
//{

///// <summary>
///// Contains static methods for interacting with files
///// </summary>
//public static class FileSystemHelper
//{
//    /// <summary>
//    /// Create a Directory
//    /// </summary>
//    public static Result<Unit, IErrorBuilder> CreateDirectory(
//        this IFileSystem fileSystem,
//        string path)
//    {
//        Result<Unit, IErrorBuilder> r;

//        try
//        {
//            fileSystem.Directory.CreateDirectory(path);
//            r = Unit.Default;
//        }
//        #pragma warning disable CA1031 // Do not catch general exception types
//        catch (Exception e)
//        {
//            r = ErrorCode.ExternalProcessError.ToErrorBuilder(e.Message);
//        }

//        return r;
//    }

//    /// <summary>
//    /// Creates a new file and writes in the text.
//    /// </summary>
//    public static async Task<Result<Unit, IErrorBuilder>> CreateFileAsync(
//        this IFileSystem fileSystem,
//        string path,
//        string text,
//        CancellationToken ca)
//    {
//        Result<Unit, IErrorBuilder> r1;

//        try
//        {
//            using var sw = fileSystem.File.CreateText(path);
//            await sw.WriteAsync(text.AsMemory(), ca);
//            r1 = Unit.Default;
//        }
//        #pragma warning disable CA1031 // Do not catch general exception types
//        catch (Exception e)
//        {
//            r1 = ErrorCode.ExternalProcessError.ToErrorBuilder(e.Message);
//        }
//        #pragma warning restore CA1031 // Do not catch general exception types

//        return r1;
//    }

//    /// <summary>
//    /// Delete a directory
//    /// </summary>
//    public static Result<Unit, IErrorBuilder> DeleteDirectory(
//        this IFileSystem fileSystem,
//        string path,
//        bool recursive)
//    {
//        try
//        {
//            fileSystem.Directory.Delete(path, recursive);
//        }
//        #pragma warning disable CA1031 // Do not catch general exception types
//        catch (Exception e)
//        {
//            return ErrorCode.ExternalProcessError.ToErrorBuilder(e.Message);
//        }
//        #pragma warning restore CA1031 // Do not catch general exception types

//        return Unit.Default;
//    }

//    /// <summary>
//    /// Delete a file
//    /// </summary>
//    public static Result<Unit, IErrorBuilder> DeleteFile(this IFileSystem fileSystem, string path)
//    {
//        try
//        {
//            fileSystem.File.Delete(path);
//        }
//        #pragma warning disable CA1031 // Do not catch general exception types
//        catch (Exception e)
//        {
//            return ErrorCode.ExternalProcessError.ToErrorBuilder(e.Message);
//        }
//        #pragma warning restore CA1031 // Do not catch general exception types

//        return Unit.Default;
//    }

//    /// <summary>
//    /// Read a file
//    /// </summary>
//    public static Result<IStream, IErrorBuilder> ReadFile(
//        this IFileSystem fileSystem,
//        string path,
//        bool decompress)
//    {
//        Result<IStream, IErrorBuilder> result;

//        try

//        {
//            IStream fs = fileSystem.File.OpenRead(path);

//            if (decompress)
//            {
//                fs = fileSystem.Compression.Decompress(fs);
//            }

//            result = Result.Success<IStream, IErrorBuilder>(fs);
//        }
//        #pragma warning disable CA1031 // Do not catch general exception types
//        catch (Exception e)
//        {
//            result = ErrorCode.ExternalProcessError.ToErrorBuilder(e.Message);
//        }
//        #pragma warning restore CA1031 // Do not catch general exception types

//        return result;
//    }

//    /// <summary>
//    /// Extract a file to a directory
//    /// </summary>
//    public static Result<Unit, IErrorBuilder> ExtractToDirectory(
//        this IFileSystem fileSystem,
//        string sourceArchivePath,
//        string destinationDirectoryPath,
//        bool overwrite)
//    {
//        Maybe<IErrorBuilder> error;

//        try
//        {
//            fileSystem.Compression.ExtractToDirectory(
//                sourceArchivePath,
//                destinationDirectoryPath,
//                overwrite
//            );

//            error = Maybe<IErrorBuilder>.None;
//        }
//        #pragma warning disable CA1031 // Do not catch general exception types
//        catch (Exception e)
//        {
//            error = Maybe<IErrorBuilder>.From(
//                ErrorCode.ExternalProcessError.ToErrorBuilder(e.Message)
//            );
//        }
//        #pragma warning restore CA1031 // Do not catch general exception types

//        if (error.HasValue)
//            return Result.Failure<Unit, IErrorBuilder>(error.Value);

//        return Unit.Default;
//    }

//    /// <summary>
//    /// Writes a file asynchronously
//    /// </summary>
//    public static async Task<Result<Unit, IErrorBuilder>> WriteFileAsync(
//        this IFileSystem fileSystem,
//        string path,
//        IStream stream,
//        bool compress,
//        CancellationToken cancellationToken)
//    {
//        Maybe<IErrorBuilder> error;

//        try
//        {
//            IStream writeStream = fileSystem.File.Create(path);

//            if (compress)
//                writeStream = fileSystem.Compression.Compress(writeStream);

//            await stream.CopyToAsync(writeStream, cancellationToken);
//            writeStream.Close();
//            error = Maybe<IErrorBuilder>.None;
//        }
//        #pragma warning disable CA1031 // Do not catch general exception types
//        catch (Exception e)
//        {
//            error = Maybe<IErrorBuilder>.From(
//                ErrorCode.ExternalProcessError.ToErrorBuilder(e.Message)
//            );
//        }
//        #pragma warning restore CA1031 // Do not catch general exception types

//        if (error.HasValue)
//            return Result.Failure<Unit, IErrorBuilder>(error.Value);

//        return Unit.Default;
//    }
//}

//}


