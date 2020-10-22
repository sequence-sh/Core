using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Returns whether a file on the file system contains a particular string.
    /// </summary>
    public class DoesFileContain : CompoundStep<bool>
    {
        /// <inheritdoc />
        public override async Task<Result<bool, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var pathResult = await Path.Run(stateMonad, cancellationToken);

            if (pathResult.IsFailure) return pathResult.ConvertFailure<bool>();

            if (!File.Exists(pathResult.Value))
                return new SingleError($"File '{pathResult.Value}' does not exist",
                    ErrorCode.ExternalProcessError, new StepErrorLocation(this));

            var textResult = await Text.Run(stateMonad, cancellationToken);
            if (textResult.IsFailure) return textResult.ConvertFailure<bool>();

            Maybe<SingleError> error;
            string realText;

            try
            {
                realText = await File.ReadAllTextAsync(pathResult.Value, cancellationToken);
                error = Maybe<SingleError>.None;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                realText = "";
                error = Maybe<SingleError>.From(new SingleError(e.Message, ErrorCode.ExternalProcessError, new StepErrorLocation(this)));
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (error.HasValue)
                return error.Value;

            var r = realText.Contains(textResult.Value);

            return r;

        }

        /// <summary>
        /// The path to the file to check.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Path { get; set; } = null!;


        /// <summary>
        /// The text to check for.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Text { get; set; } = null!;


        /// <inheritdoc />
        public override IStepFactory StepFactory => DoesFileContainStepFactory.Instance;
    }

    /// <summary>
    /// Returns whether a file on the file system contains a particular string.
    /// </summary>
    public class DoesFileContainStepFactory : SimpleStepFactory<DoesFileContain, bool>
    {
        private DoesFileContainStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<DoesFileContain, bool> Instance { get; } = new DoesFileContainStepFactory();
    }
}
