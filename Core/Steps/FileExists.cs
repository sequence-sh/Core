using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Returns whether a file on the file system exists.
    /// </summary>
    public class FileExists : CompoundStep<bool>
    {
        /// <summary>
        /// The path to the file to check.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Path { get; set; } = null!;

        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(StateMonad stateMonad)
        {
            var pathResult = Path.Run(stateMonad);

            if (pathResult.IsFailure) return pathResult.ConvertFailure<bool>();

            var r = File.Exists(pathResult.Value);

            return r;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => FileExistsStepFactory.Instance;
    }

    /// <summary>
    /// Returns whether a file on the file system exists.
    /// </summary>
    public class FileExistsStepFactory : SimpleStepFactory<FileExists, bool>
    {
        private FileExistsStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<FileExists, bool> Instance { get; } = new FileExistsStepFactory();
    }
}