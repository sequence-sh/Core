using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    /// Splits a string.
    /// </summary>
    [Alias("SplitString")]
    public sealed class StringSplit : CompoundStep<Core.Sequence<StringStream>>
    {
        /// <summary>
        /// The string to split.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<StringStream> String { get; set; } = null!;

        /// <summary>
        /// The delimiter to use.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<StringStream> Delimiter { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<Core.Sequence<StringStream>, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var stringResult = await String.Run(stateMonad, cancellationToken)
                .Map(async x=> await x.GetStringAsync());

            if (stringResult.IsFailure) return stringResult.ConvertFailure<Core.Sequence<StringStream>>();

            var delimiterResult = await  Delimiter.Run(stateMonad, cancellationToken)
                .Map(async x=> await x.GetStringAsync());

            if (delimiterResult.IsFailure) return delimiterResult.ConvertFailure<Core.Sequence<StringStream>>();


            var results = stringResult.Value
                .Split(new[] {delimiterResult.Value}, StringSplitOptions.None)
                .Select(x => new StringStream(x))
                .ToList().ToSequence();

            return results;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => StringSplitStepFactory.Instance;
    }

    /// <summary>
    /// Splits a string.
    /// </summary>
    public class StringSplitStepFactory : SimpleStepFactory<StringSplit, Core.Sequence<StringStream>>
    {
        private StringSplitStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<StringSplit, Core.Sequence<StringStream>> Instance { get; } = new StringSplitStepFactory();
    }
}
