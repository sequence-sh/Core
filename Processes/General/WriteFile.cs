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
    /// Writes a file to the local file system.
    /// </summary>
    public sealed class WriteFileProcessFactory : SimpleRunnableProcessFactory<WriteFile, Unit>
    {
        private WriteFileProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<WriteFile, Unit> Instance { get; } = new WriteFileProcessFactory();
    }


    /// <summary>
    /// Writes a file to the local file system.
    /// </summary>
    public sealed class WriteFile  : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState)
        {
            var data = Folder.Run(processState).Compose(() => FileName.Run(processState),()=> Text.Run(processState));

            if (data.IsFailure)
                return data.ConvertFailure<Unit>();


            var path = Path.Combine(data.Value.Item1, data.Value.Item2);

            Maybe<IRunErrors> errors;
            try
            {
                File.WriteAllText(path, data.Value.Item3);
                errors = Maybe<IRunErrors>.None;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                errors = Maybe<IRunErrors>.From(new RunError(e.Message, Name, null, ErrorCode.ExternalProcessError));
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (errors.HasValue)
                return Result.Failure<Unit, IRunErrors>(errors.Value);
            return Unit.Default;

        }


        /// <summary>
        /// The name of the folder.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> Folder { get; set; } = null!;

        /// <summary>
        /// The name of the file to write to.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> FileName { get; set; } = null!;

        /// <summary>
        /// The text to write.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> Text { get; set; } = null!;


        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => WriteFileProcessFactory.Instance;
    }
}
