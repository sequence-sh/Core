using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Writes a file to the local file system.
    /// </summary>
    public sealed class WriteFile  : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            var data = Folder.Run(stateMonad).Compose(() => FileName.Run(stateMonad),()=> Text.Run(stateMonad));

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
        [StepProperty]
        [Required]
        public IStep<string> Folder { get; set; } = null!;

        /// <summary>
        /// The name of the file to write to.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> FileName { get; set; } = null!;

        /// <summary>
        /// The text to write.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Text { get; set; } = null!;


        /// <inheritdoc />
        public override IStepFactory StepFactory => WriteFileStepFactory.Instance;
    }

    /// <summary>
    /// Writes a file to the local file system.
    /// </summary>
    public sealed class WriteFileStepFactory : SimpleStepFactory<WriteFile, Unit>
    {
        private WriteFileStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<WriteFile, Unit> Instance { get; } = new WriteFileStepFactory();
    }
}
