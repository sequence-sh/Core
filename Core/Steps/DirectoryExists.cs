using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Returns whether a directory on the file system exists.
    /// </summary>
    public class DirectoryExists : CompoundStep<bool>
    {
        /// <summary>
        /// The path to the folder to check.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Path { get; set; } = null!;

        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(StateMonad stateMonad)
        {
            var pathResult = Path.Run(stateMonad);

            if (pathResult.IsFailure) return pathResult.ConvertFailure<bool>();

            Result<bool, IRunErrors> r;
            try
            {
                r = Directory.Exists(pathResult.Value);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                r = new RunError(e.Message, nameof(DirectoryExists), null, ErrorCode.ExternalProcessError);
            }
#pragma warning restore CA1031 // Do not catch general exception types


            return r;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => DirectoryExistsStepFactory.Instance;
    }

    /// <summary>
    /// Returns whether a directory on the file system exists.
    /// </summary>
    public class DirectoryExistsStepFactory : SimpleStepFactory<DirectoryExists, bool>
    {
        private DirectoryExistsStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<DirectoryExists, bool> Instance { get; } = new DirectoryExistsStepFactory();
    }

}