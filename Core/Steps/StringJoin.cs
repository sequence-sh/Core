using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Join strings with a delimiter.
    /// </summary>
    public sealed class StringJoin : CompoundStep<string>
    {
        /// <summary>
        /// The delimiter to use.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<string> Delimiter { get; set; } = null!;

        /// <summary>
        /// The string to join.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<List<string>> Strings { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<string, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var list = await Strings.Run(stateMonad, cancellationToken);
            if (list.IsFailure) return list.ConvertFailure<string>();

            var delimiter = await Delimiter.Run(stateMonad, cancellationToken);
            if (delimiter.IsFailure) return delimiter;


            var result = string.Join(delimiter.Value, list.Value);

            return result;

        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => StringJoinStepFactory.Instance;
    }


    /// <summary>
    /// Join strings with a delimiter.
    /// </summary>
    public sealed class StringJoinStepFactory : SimpleStepFactory<StringJoin, string>
    {
        private StringJoinStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<StringJoin, string> Instance { get; } = new StringJoinStepFactory();
    }
}