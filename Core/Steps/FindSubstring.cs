using System;
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
    /// Gets the index of the first instance of a substring in a string.
    /// Returns -1 if the substring is not present.
    /// </summary>
    public sealed class FindSubstring : CompoundStep<int>
    {
        /// <summary>
        /// The string to check.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<StringStream> String { get; set; } = null!;

        /// <summary>
        /// The substring to find.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<StringStream> SubString { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<int, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var str = await String.Run(stateMonad, cancellationToken)
                .Map(async x=> await x.GetStringAsync());
            if (str.IsFailure) return str.ConvertFailure<int>();

            var subString = await SubString.Run(stateMonad, cancellationToken)
                .Map(async x=> await x.GetStringAsync());
            if (subString.IsFailure) return subString.ConvertFailure<int>();


            return str.Value.IndexOf(subString.Value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => FindSubstringStepFactory.Instance;
    }


    /// <summary>
    /// Gets the first instance of a substring in a string.
    /// Returns -1 if the substring is not present.
    /// </summary>
    public sealed class FindSubstringStepFactory : SimpleStepFactory<FindSubstring, int>
    {
        private FindSubstringStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<FindSubstring, int> Instance { get; } = new FindSubstringStepFactory();
    }


}