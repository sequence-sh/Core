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

            var isRelative = await IsRelative.Run(stateMonad, cancellationToken);
            if (isRelative.IsFailure) return isRelative.ConvertFailure<string>();

            var paths = pathsResult.Value.AsEnumerable();

            if (isRelative.Value)
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

        /// <summary>
        /// Whether the paths should be relative to the current working directory
        /// </summary>
        [StepProperty(Order = 2)]
        [DefaultValueExplanation("false")]
        public IStep<bool> IsRelative { get; set; } = new Constant<bool>(false);

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