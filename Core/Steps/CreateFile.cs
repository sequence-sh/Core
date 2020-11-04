using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Creates a file.
    /// </summary>
    public class CreateFile : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var pathResult = await Path.Run(stateMonad, cancellationToken);

            if (pathResult.IsFailure)
                return pathResult.ConvertFailure<Unit>();

            var textResult = await Text.Run(stateMonad, cancellationToken);

            if (textResult.IsFailure)
                return textResult.ConvertFailure<Unit>();

            var result = await stateMonad.FileSystemHelper.CreateFileAsync(pathResult.Value, textResult.Value, cancellationToken)
                .MapError(x=>x.WithLocation(this));

            return result;
        }

        /// <summary>
        /// The path to the file to create.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Path { get; set; } = null!;


        /// <summary>
        /// The text to put in the file.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Text { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => CreateFileStepFactory.Instance;
    }

    /// <summary>
    /// Creates a file.
    /// </summary>
    public class CreateFileStepFactory : SimpleStepFactory<CreateFile, Unit>
    {
        private CreateFileStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<CreateFile, Unit> Instance { get; } = new CreateFileStepFactory();
    }
}