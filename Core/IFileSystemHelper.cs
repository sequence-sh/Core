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
    }




    /// <summary>
    /// Contains methods to interact with the file system.
    /// </summary>
    public interface IFileSystemHelper
    {

    }
}