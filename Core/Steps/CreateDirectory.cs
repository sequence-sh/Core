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
    /// Creates a new directory in the file system.
    /// Will create all directories and subdirectories in the specified path unless they already exist.
    /// </summary>
    [Alias("mkdir")]
    public class CreateDirectory : CompoundStep<Unit>
    {
        /// <inheritdoc />
        protected override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var pathResult = await Path.Run(stateMonad, cancellationToken);

            if (pathResult.IsFailure)
                return pathResult.ConvertFailure<Unit>();

            var pathString = await pathResult.Value.GetStringAsync();

            var r = stateMonad.FileSystemHelper.CreateDirectory(pathString)
                .MapError(x => x.WithLocation(this));

            return r;
        }
        
        /// <summary>
        /// The path to the directory to create.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<StringStream> Path { get; set; } = null!;
        
        /// <inheritdoc />
        public override IStepFactory StepFactory => CreateDirectoryStepFactory.Instance;
    }

    /// <summary>
    /// Creates a new directory in the file system.
    /// Will create all directories and subdirectories in the specified path unless they already exist.
    /// </summary>
    public class CreateDirectoryStepFactory : SimpleStepFactory<CreateDirectory, Unit>
    {
        private CreateDirectoryStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<CreateDirectory, Unit> Instance { get; } = new CreateDirectoryStepFactory();
    }
}
