using System.Collections.Generic;
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
    /// Extracts entities from a Concordance stream.
    /// The same as FromCSV but with different default values.
    /// </summary>
    public sealed class FromConcordance : CompoundStep<IAsyncEnumerable<Entity>>
    {
        /// <inheritdoc />
        public override async Task<Result<IAsyncEnumerable<Entity>, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var result = await CSVReader.ReadCSV(
                stateMonad,
                Stream,
                Delimiter,
                new StringConstant(new StringStream("")),
                QuoteCharacter,
                MultiValueDelimiter,
                new StepErrorLocation(this),
                cancellationToken);

            return result;
        }


        /// <summary>
        /// Stream containing the CSV data.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<StringStream> Stream { get; set; } = null!;


        /// <summary>
        /// The delimiter to use to separate fields.
        /// </summary>
        [StepProperty(2)]
        [DefaultValueExplanation("\\u0014 - DC4")]
        public IStep<StringStream> Delimiter { get; set; } = new StringConstant(new StringStream("\u0014"));


        /// <summary>
        /// The quote character to use.
        /// Should be a single character or an empty string.
        /// If it is empty then strings cannot be quoted.
        /// </summary>
        [StepProperty(3)]
        [DefaultValueExplanation("\u00FE")]
        public IStep<StringStream> QuoteCharacter { get; set; } = new StringConstant(new StringStream("\u00FE"));

        /// <summary>
        /// The multi value delimiter character to use.
        /// Should be a single character or an empty string.
        /// If it is empty then fields cannot have multiple fields.
        /// </summary>
        [StepProperty(4)]
        [DefaultValueExplanation("|")]
        public IStep<StringStream> MultiValueDelimiter { get; set; } = new StringConstant(new StringStream("|"));

        /// <inheritdoc />
        public override IStepFactory StepFactory => FromConcordanceStepFactory.Instance;
    }

    /// <summary>
    /// Extracts entities from a Concordance stream.
    /// The same as FromCSV but with different default values.
    /// </summary>
    public sealed class FromConcordanceStepFactory : SimpleStepFactory<FromConcordance, IAsyncEnumerable<Entity>>
    {
        private FromConcordanceStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<FromConcordance, IAsyncEnumerable<Entity>> Instance { get; } = new FromConcordanceStepFactory();
    }
}