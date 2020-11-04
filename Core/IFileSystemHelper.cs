using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{
    /// <summary>
    /// Interacts with the file system in the normal way.
    /// </summary>
    public class FileSystemHelper : IFileSystemHelper
    {
        private FileSystemHelper() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static IFileSystemHelper Instance { get; } = new FileSystemHelper();

        /// <inheritdoc />
        public Result<Unit, IErrorBuilder> CreateDirectory(string path)
        {
            Result<Unit, IErrorBuilder> r;

            try
            {
                Directory.CreateDirectory(path);
                r = Unit.Default;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                r = new ErrorBuilder(e.Message, ErrorCode.ExternalProcessError);
            }

            return r;
        }

        /// <summary>
        /// Creates a new file and writes in the text.
        /// </summary>
        public async Task<Result<Unit, IErrorBuilder>> CreateFileAsync(string path, string text, CancellationToken ca)
        {
            Result<Unit, IErrorBuilder> r1;

            try
            {
                await using var sw = File.CreateText(path);
                await sw.WriteAsync(text.AsMemory(), ca);
                r1 = Unit.Default;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                r1 = new ErrorBuilder(e.Message, ErrorCode.ExternalProcessError);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return r1;
        }

        /// <inheritdoc />
        public bool DoesFileExist(string path) => File.Exists(path);

        /// <inheritdoc />
        public bool DoesDirectoryExist(string path) => Directory.Exists(path);

        /// <inheritdoc />
        public Result<Unit, IErrorBuilder> DeleteDirectory(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                return new ErrorBuilder(e, ErrorCode.ExternalProcessError);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return Unit.Default;
        }

        /// <inheritdoc />
        public Result<Unit, IErrorBuilder> DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                return new ErrorBuilder(e, ErrorCode.ExternalProcessError);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return Unit.Default;
        }

        /// <inheritdoc />
        public Result<Stream, IErrorBuilder> ReadFile(string path)
        {
            Result<Stream, IErrorBuilder> result;
            try
            {
                Stream fs = File.OpenRead(path);
                result = fs;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                result = new ErrorBuilder(e.Message, ErrorCode.ExternalProcessError);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }
    }




    /// <summary>
    /// Contains methods to interact with the file system.
    /// </summary>
    public interface IFileSystemHelper
    {
        /// <summary>
        /// Create a new directory.
        /// </summary>
        Result<Unit, IErrorBuilder> CreateDirectory(string path);

        /// <summary>
        /// Creates a new file and writes in the text.
        /// </summary>
        Task<Result<Unit, IErrorBuilder>> CreateFileAsync(string path, string text, CancellationToken ca);

        /// <summary>
        /// Does a file with this path exist.
        /// </summary>
        bool DoesFileExist(string path);

        /// <summary>
        /// Does a directory with this path exist.
        /// </summary>
        bool DoesDirectoryExist(string path);

        /// <summary>
        /// Deletes a directory.
        /// </summary>
        Result<Unit, IErrorBuilder> DeleteDirectory(string path, bool recursive);

        /// <summary>
        /// Deletes a file.
        /// </summary>
        Result<Unit, IErrorBuilder> DeleteFile(string path);

        /// <summary>
        /// Reads the text of a file
        /// </summary>
        Result<Stream, IErrorBuilder> ReadFile(string path);
    }
}