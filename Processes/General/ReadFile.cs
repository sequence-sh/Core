using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Reads text from a file.
    /// </summary>
    public sealed class ReadFileProcessFactory : SimpleRunnableProcessFactory<ReadFile, string>
    {
        private ReadFileProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<ReadFile, string> Instance { get; } = new ReadFileProcessFactory();
    }


    /// <summary>
    /// Reads text from a file.
    /// </summary>
    public sealed class ReadFile : CompoundRunnableProcess<string>
    {
        /// <inheritdoc />
        public override Result<string, IRunErrors> Run(ProcessState processState)
        {
            var data = Folder.Run(processState).Compose(() => FileName.Run(processState));

            if (data.IsFailure)
                return data.ConvertFailure<string>();


            var path = Path.Combine(data.Value.Item1, data.Value.Item2);

            Result<string, IRunErrors> result;
            try
            {
                result = File.ReadAllText(path);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                result = new RunError(e.Message, Name, null, ErrorCode.ExternalProcessError);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }


        /// <summary>
        /// The name of the folder.
        /// </summary>
        [RunnableProcessPropertyAttribute]
        [Required]
        public IRunnableProcess<string> Folder { get; set; } = null!;

        /// <summary>
        /// The name of the file to write to.
        /// </summary>
        [RunnableProcessPropertyAttribute]
        [Required]
        public IRunnableProcess<string> FileName { get; set; } = null!;

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => ReadFileProcessFactory.Instance;
    }
}