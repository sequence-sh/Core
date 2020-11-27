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
    /// Write entities to a stream in concordance format.
    /// The same as WriteCSV but with different default values.
    /// </summary>
    public sealed class WriteConcordance : CompoundStep<Stream>
    {
        /// <inheritdoc />
        public override async Task<Result<Stream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var result = await CSVWriter.WriteCSV(stateMonad,
                Entities,
                Delimiter,
                Encoding,
                QuoteCharacter,
                AlwaysQuote, MultiValueDelimiter,
                DateTimeFormat, new StepErrorLocation(this), cancellationToken);

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => WriteConcordanceStepFactory.Instance;

        /// <summary>
        /// The entities to write.
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<EntityStream> Entities { get; set; } = null!;

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
        [DefaultValueExplanation("\u0014")]
        public IStep<string> Delimiter { get; set; } = new Constant<string>("\u0014");

        /// <summary>
        /// The quote character to use.
        /// Should be a single character or an empty string.
        /// If it is empty then strings cannot be quoted.
        /// </summary>
        [StepProperty(Order = 4)]
        [DefaultValueExplanation("\u00FE")]
        public IStep<string> QuoteCharacter { get; set; } = new Constant<string>("\u00FE");

        /// <summary>
        /// Whether to always quote all fields and headers.
        /// </summary>
        [StepProperty(Order = 4)]
        [DefaultValueExplanation("false")]
        public IStep<bool> AlwaysQuote { get; set; } = new Constant<bool>(true);

        /// <summary>
        /// The multi value delimiter character to use.
        /// Should be a single character or an empty string.
        /// If it is empty then fields cannot have multiple fields.
        /// </summary>
        [StepProperty(Order = 6)]
        [DefaultValueExplanation("")]
        public IStep<string> MultiValueDelimiter { get; set; } = new Constant<string>("|");

        /// <summary>
        /// The format to use for DateTime fields.
        /// </summary>
        [StepProperty(Order = 7)]
        [DefaultValueExplanation("yyyy/MM/dd H:mm:ss")]
        public IStep<string> DateTimeFormat { get; set; } = new Constant<string>("yyyy/MM/dd H:mm:ss");
    }



    /// <summary>
    /// Write entities to a stream in concordance format.
    /// The same as WriteCSV but with different default values.
    /// </summary>
    public sealed class WriteConcordanceStepFactory : SimpleStepFactory<WriteConcordance, Stream>
    {
        private WriteConcordanceStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<WriteConcordance, Stream> Instance { get; } = new WriteConcordanceStepFactory();
    }
}