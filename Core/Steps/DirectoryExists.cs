using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Returns whether a directory on the file system exists.
    /// </summary>
    [Alias("DoesDirectoryExist")]
    public class DirectoryExists : CompoundStep<bool>
    {
        /// <summary>
        /// The path to the folder to check.
        /// </summary>
        [StepProperty(1)]
        [Required]
        [Alias("Directory")]
        public IStep<StringStream> Path { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<bool, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var pathResult = await Path.Run(stateMonad, cancellationToken);

            if (pathResult.IsFailure) return pathResult.ConvertFailure<bool>();

            var pathString = await pathResult.Value.GetStringAsync();

            var r = stateMonad.FileSystemHelper.DoesDirectoryExist(pathString);
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