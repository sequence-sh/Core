using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Combine Paths
    /// </summary>
    public sealed class PathCombine : CompoundStep<string>
    {
        /// <inheritdoc />
        public override async Task<Result<string, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var pathsResult = await Paths.Run(stateMonad, cancellationToken);
            if (pathsResult.IsFailure) return pathsResult.ConvertFailure<string>();


            if (pathsResult.Value.Count <= 0)
                return stateMonad.FileSystemHelper.GetCurrentDirectory();
            var paths = pathsResult.Value.AsEnumerable();

            if (!Path.IsPathFullyQualified(pathsResult.Value[0]))
                paths = paths.Prepend(stateMonad.FileSystemHelper.GetCurrentDirectory());

            var result = Path.Combine(paths.ToArray());

            return result;
        }

        /// <summary>
        /// The paths to combine.
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<List<string>> Paths { get; set; } = null!;


        /// <inheritdoc />
        public override IStepFactory StepFactory => PathCombineStepFactory.Instance;
    }

    /// <summary>
    /// Combine Paths
    /// </summary>
    public class PathCombineStepFactory : SimpleStepFactory<PathCombine, string>
    {
        private PathCombineStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<PathCombine, string> Instance { get; } = new PathCombineStepFactory();
    }
}