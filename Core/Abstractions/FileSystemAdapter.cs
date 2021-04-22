//using Reductech.EDR.Core.ExternalProcesses;
//using Thinktecture.IO;
//using Thinktecture.IO.Adapters;

//namespace Reductech.EDR.Core.Abstractions
//{

///// <summary>
///// Adapter for the file system
///// </summary>
//public record FileSystemAdapter(
//        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
//        IDirectory Directory,
//        IFile File,
//        ICompression Compression) : IFileSystem
//    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
//{
//    /// <summary>
//    /// The default adapter.
//    /// </summary>
//    public static readonly FileSystemAdapter Default = new(
//        new DirectoryAdapter(),
//        new FileAdapter(),
//        new CompressionAdapter()
//    );
//}

//}


