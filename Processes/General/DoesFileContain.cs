using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Returns whether a file on the file system contains a particular string.
    /// </summary>
    public class DoesFileContainProcessFactory : SimpleRunnableProcessFactory<DoesFileContain, bool>
    {
        private DoesFileContainProcessFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleRunnableProcessFactory<DoesFileContain, bool> Instance { get; } = new DoesFileContainProcessFactory();
    }

    /// <summary>
    /// Returns whether a file on the file system contains a particular string.
    /// </summary>
    public class DoesFileContain : CompoundRunnableProcess<bool>
    {
        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(ProcessState processState)
        {
            var pathResult = Path.Run(processState);

            if (pathResult.IsFailure) return pathResult.ConvertFailure<bool>();

            if (!File.Exists(pathResult.Value))
                return new RunError($"File '{pathResult.Value}' does not exist", Name, null,
                    ErrorCode.ExternalProcessError);

            var textResult = Text.Run(processState);
            if (textResult.IsFailure) return textResult.ConvertFailure<bool>();

            Maybe<RunError> error;
            string realText;

            try
            {
                realText = File.ReadAllText(pathResult.Value);
                error = Maybe<RunError>.None;
            }
            catch (Exception e)
            {
                realText = "";
                error = Maybe<RunError>.From(new RunError(e.Message, Name, null, ErrorCode.ExternalProcessError));
            }

            if (error.HasValue)
                return error.Value;

            var r = realText.Contains(textResult.Value);

            return r;

        }

        /// <summary>
        /// The path to the file or folder to check.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> Path { get; set; } = null!;


        /// <summary>
        /// The text to check for.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> Text { get; set; } = null!;


        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => DoesFileContainProcessFactory.Instance;
    }
}
