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
    public sealed class StringSplit : CompoundStep<IAsyncEnumerable<StringStream>>
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
        public override async Task<Result<IAsyncEnumerable<StringStream>, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var stringResult = await String.Run(stateMonad, cancellationToken)
                .Map(async x=> await x.GetStringAsync());

            if (stringResult.IsFailure) return stringResult.ConvertFailure<IAsyncEnumerable<StringStream>>();

            var delimiterResult = await  Delimiter.Run(stateMonad, cancellationToken)
                .Map(async x=> await x.GetStringAsync());

            if (delimiterResult.IsFailure) return delimiterResult.ConvertFailure<IAsyncEnumerable<StringStream>>();


            var results = stringResult.Value
                .Split(new[] {delimiterResult.Value}, StringSplitOptions.None)
                .Select(x => new StringStream(x))
                .ToList().ToAsyncEnumerable();


            return Result.Success<IAsyncEnumerable<StringStream>, IError>(results);
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => StringSplitStepFactory.Instance;
    }

    /// <summary>
    /// Splits a string.
    /// </summary>
    public class StringSplitStepFactory : SimpleStepFactory<StringSplit, IAsyncEnumerable<StringStream>>
    {
        private StringSplitStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<StringSplit, IAsyncEnumerable<StringStream>> Instance { get; } = new StringSplitStepFactory();
    }
}