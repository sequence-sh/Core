using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Extracts entities from a CSV file.
    /// The same as FromConcordance but with different default values.
    /// </summary>
    public sealed class FromCSV : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
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
        public IStep<DataStream> Stream { get; set; } = null!;

        /// <summary>
        /// How the stream is encoded.
        /// </summary>
        [StepProperty(Order = 2)]
        [DefaultValueExplanation("UTF8 no BOM")]
        public IStep<EncodingEnum> Encoding { get; set; } = new Constant<EncodingEnum>(EncodingEnum.UTF8);

        /// <summary>
        /// The delimiter to use to separate fields.
        /// </summary>
        [StepProperty(Order = 3)]
        [DefaultValueExplanation(",")]
        public IStep<string> Delimiter { get; set; } = new Constant<string>(",");

        /// <summary>
        /// The token to use to indicate comments.
        /// Must be a single character, or an empty string.
        /// If it is empty, then comments cannot be indicated
        /// </summary>
        [StepProperty(Order = 4)]
        [DefaultValueExplanation("#")]
        public IStep<string> CommentCharacter { get; set; } = new Constant<string>("#");

        /// <summary>
        /// The quote character to use.
        /// Should be a single character or an empty string.
        /// If it is empty then strings cannot be quoted.
        /// </summary>
        [StepProperty(Order = 5)]
        [DefaultValueExplanation("\"")]
        public IStep<string> QuoteCharacter { get; set; } = new Constant<string>("\"");


        /// <summary>
        /// The multi value delimiter character to use.
        /// Should be a single character or an empty string.
        /// If it is empty then fields cannot have multiple fields.
        /// </summary>
        [StepProperty(Order = 6)]
        [DefaultValueExplanation("")]
        public IStep<string> MultiValueDelimiter { get; set; } = new Constant<string>("");

        /// <inheritdoc />
        public override IStepFactory StepFactory => FromCSVStepFactory.Instance;
    }


    /// <summary>
    /// Extracts entities from a CSV Stream
    /// The same as FromConcordance but with different default values.
    /// </summary>
    public sealed class FromCSVStepFactory : SimpleStepFactory<FromCSV, EntityStream>
    {
        private FromCSVStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<FromCSV, EntityStream> Instance { get; } = new FromCSVStepFactory();
    }
}