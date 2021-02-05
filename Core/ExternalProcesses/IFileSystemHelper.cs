using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.ExternalProcesses
{

///// <summary>
///// Contains methods to interact with the file system.
///// </summary>
//public interface IFileSystemHelper
//{
//    /// <summary>
//    /// Create a new directory.
//    /// </summary>
//    Result<Unit, IErrorBuilder> CreateDirectory(string path);

//    /// <summary>
//    /// Creates a new file and writes in the text.
//    /// </summary>
//    Task<Result<Unit, IErrorBuilder>> CreateFileAsync(
//        string path,
//        string text,
//        CancellationToken ca);

//    /// <summary>
//    /// Does a file with this path exist.
//    /// </summary>
//    bool DoesFileExist(string path);

//    /// <summary>
//    /// Does a directory with this path exist.
//    /// </summary>
//    bool DoesDirectoryExist(string path);

//    /// <summary>
//    /// Deletes a directory.
//    /// </summary>
//    Result<Unit, IErrorBuilder> DeleteDirectory(string path, bool recursive);

//    /// <summary>
//    /// Deletes a file.
//    /// </summary>
//    Result<Unit, IErrorBuilder> DeleteFile(string path);

//    /// <summary>
//    /// Reads the text of a file
//    /// </summary>
//    Result<Stream, IErrorBuilder> ReadFile(string path, bool decompress);

//    /// <summary>
//    /// Extracts all the files in the specified archive to the directory on the file system.
//    /// </summary>
//    Result<Unit, IErrorBuilder> ExtractToDirectory(
//        string sourceArchivePath,
//        string destinationDirectoryPath,
//        bool overwrite);

//    /// <summary>
//    /// Creates a file and writes a stream to it.
//    /// </summary>
//    Task<Result<Unit, IErrorBuilder>> WriteFileAsync(
//        string path,
//        Stream text,
//        bool decompress,
//        CancellationToken cancellationToken);

//    /// <summary>
//    /// Gets the current working directory of the application.
//    /// </summary>
//    string GetCurrentDirectory();
//}

}
