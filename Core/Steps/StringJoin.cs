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
    /// Join strings with a delimiter.
    /// </summary>
    [Alias("JoinStrings")]
    public sealed class StringJoin : CompoundStep<StringStream>
    {
        /// <summary>
        /// The delimiter to use.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<StringStream> Delimiter { get; set; } = null!;

        /// <summary>
        /// The string to join.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<Core.Array<StringStream>> Strings { get; set; } = null!;

        /// <inheritdoc />
        protected override async Task<Result<StringStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var listResult = await Strings.Run(stateMonad, cancellationToken)
                    .Bind(x=>x.GetElementsAsync(cancellationToken));

            if (listResult.IsFailure) return listResult.ConvertFailure<StringStream>();

            var delimiter = await Delimiter.Run(stateMonad, cancellationToken)
                .Map(async x=> await x.GetStringAsync());
            if (delimiter.IsFailure) return delimiter.ConvertFailure<StringStream>();

            if (listResult.Value.Count == 0)
                return new StringStream(string.Empty);

            if (listResult.Value.Count == 1)
                return listResult.Value.Single();

            var strings = new List<string>();

            foreach (var stringStream in listResult.Value)
                strings.Add(await stringStream.GetStringAsync());


            var resultString = string.Join(delimiter.Value, strings);

            return new StringStream(resultString);

        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => StringJoinStepFactory.Instance;
    }


    /// <summary>
    /// Join strings with a delimiter.
    /// </summary>
    public sealed class StringJoinStepFactory : SimpleStepFactory<StringJoin, StringStream>
    {
        private StringJoinStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<StringJoin, StringStream> Instance { get; } = new StringJoinStepFactory();
    }
}