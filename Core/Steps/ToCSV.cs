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
    /// Write entities to a stream in CSV format.
    /// The same as ToConcordance but with different default values.
    /// </summary>
    public sealed class ToCSV : CompoundStep<DataStream>
    {
        /// <inheritdoc />
        public override async Task<Result<DataStream, IError>> Run(IStateMonad stateMonad,
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
        [DefaultValueExplanation(",")]
        public IStep<string> Delimiter { get; set; } = new Constant<string>(",");

        /// <summary>
        /// The quote character to use.
        /// Should be a single character or an empty string.
        /// If it is empty then strings cannot be quoted.
        /// </summary>
        [StepProperty(Order = 4)]
        [DefaultValueExplanation("\"")]
        public IStep<string> QuoteCharacter { get; set; } = new Constant<string>("\"");

        /// <summary>
        /// Whether to always quote all fields and headers.
        /// </summary>
        [StepProperty(Order = 4)]
        [DefaultValueExplanation("false")]
        public IStep<bool> AlwaysQuote { get; set; } = new Constant<bool>(false);

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
        [DefaultValueExplanation("O - ISO 8601 compliant - e.g. 2009-06-15T13:45:30.0000000-07:00")]
        [Example("yyyy/MM/dd HH:mm:ss")]
        public IStep<string> DateTimeFormat { get; set; } = new Constant<string>("O");

        /// <inheritdoc />
        public override IStepFactory StepFactory => ToCSVFactory.Instance;
    }

    /// <summary>
    /// Write entities to a stream in CSV format.
    /// The same as ToConcordance but with different default values.
    /// </summary>
    public sealed class ToCSVFactory : SimpleStepFactory<ToCSV, DataStream>
    {
        private ToCSVFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ToCSV, DataStream> Instance { get; } = new ToCSVFactory();
    }
}
