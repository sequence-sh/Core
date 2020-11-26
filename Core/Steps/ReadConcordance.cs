using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Extracts entities from a Concordance stream.
    /// The same as ReadCSV but with different default values.
    /// </summary>
    public sealed class ReadConcordance : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var result = await CSVReader.ReadCSV(
                stateMonad,
                Stream,
                Delimiter,
                Encoding,
                CommentCharacter,
                QuoteCharacter,
                MultiValueDelimiter,
                new StepErrorLocation(this),
                cancellationToken);

            return result;
        }


        /// <summary>
        /// Stream containing the CSV data.
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<Stream> Stream { get; set; } = null!;

        /// <summary>
        /// How the stream is encoded.
        /// </summary>
        [StepProperty(Order = 2)]
        [DefaultValueExplanation("The default encoding")]
        public IStep<EncodingEnum> Encoding { get; set; } = new Constant<EncodingEnum>(EncodingEnum.Default);

        /// <summary>
        /// The delimiter to use to separate fields.
        /// </summary>
        [StepProperty(Order = 3)]
        [DefaultValueExplanation("\\u0014")]
        public IStep<string> Delimiter { get; set; } = new Constant<string>("\u0014");

        /// <summary>
        /// The token to use to indicate comments.
        /// Must be a single character, or an empty string.
        /// If it is empty, then comments cannot be indicated
        /// </summary>
        [StepProperty(Order = 4)]
        [DefaultValueExplanation("#")]
        public IStep<string> CommentCharacter { get; set; } = new Constant<string>("#"); //TODO enable char property type

        /// <summary>
        /// The quote character to use.
        /// Should be a single character or an empty string.
        /// If it is empty then strings cannot be quoted.
        /// </summary>
        [StepProperty(Order = 5)]
        [DefaultValueExplanation("\u00FE")]
        public IStep<string> QuoteCharacter { get; set; } = new Constant<string>("\u00FE");

        /// <summary>
        /// The multi value delimiter character to use.
        /// Should be a single character or an empty string.
        /// If it is empty then fields cannot have multiple fields.
        /// </summary>
        [StepProperty(Order = 6)]
        [DefaultValueExplanation("|")]
        public IStep<string> MultiValueDelimiter { get; set; } = new Constant<string>("|");

        /// <inheritdoc />
        public override IStepFactory StepFactory => ReadConcordanceStepFactory.Instance;
    }

    /// <summary>
    /// Extracts entities from a Concordance stream.
    /// The same as ReadCSV but with different default values.
    /// </summary>
    public sealed class ReadConcordanceStepFactory : SimpleStepFactory<ReadConcordance, EntityStream>
    {
        private ReadConcordanceStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ReadConcordance, EntityStream> Instance { get; } = new ReadConcordanceStepFactory();
    }
}