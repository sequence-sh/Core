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
    public sealed class JoinStrings : CompoundStep<string>
    {
        /// <summary>
        /// The delimiter to use.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Delimiter { get; set; } = null!;

        /// <summary>
        /// The elements to iterate over.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<string>> List { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<string, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var list = await List.Run(stateMonad, cancellationToken);
            if (list.IsFailure) return list.ConvertFailure<string>();

            var delimiter = await Delimiter.Run(stateMonad, cancellationToken);
            if (delimiter.IsFailure) return delimiter;


            var result = string.Join(delimiter.Value, list.Value);

            return result;

        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => JoinStringsStepFactory.Instance;
    }


    /// <summary>
    /// Join strings with a delimiter.
    /// </summary>
    public sealed class JoinStringsStepFactory : SimpleStepFactory<JoinStrings, string>
    {
        private JoinStringsStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<JoinStrings, string> Instance { get; } = new JoinStringsStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Join [{nameof(JoinStrings.List)}]");
    }
}