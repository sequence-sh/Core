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
    /// Write entities to a stream in concordance format.
    /// The same as ToCSV but with different default values.
    /// </summary>
    [Alias("ConvertEntityToConcordance")]
    public sealed class ToConcordance : CompoundStep<StringStream>
    {
        /// <inheritdoc />
        public override async Task<Result<StringStream, IError>> Run(IStateMonad stateMonad,
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
        public override IStepFactory StepFactory => ToConcordanceStepFactory.Instance;

        /// <summary>
        /// The entities to write.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<AsyncList<Entity>> Entities { get; set; } = null!;

        /// <summary>
        /// How the stream is encoded.
        /// </summary>
        [StepProperty(2)]
        [DefaultValueExplanation("UTF8 no BOM")]
        public IStep<EncodingEnum> Encoding { get; set; } = new EnumConstant<EncodingEnum>(EncodingEnum.UTF8);

        /// <summary>
        /// The delimiter to use to separate fields.
        /// </summary>
        [StepProperty(3)]
        [DefaultValueExplanation("\u0014")]
        public IStep<StringStream> Delimiter { get; set; } = new StringConstant(new StringStream( "\u0014"));

        /// <summary>
        /// The quote character to use.
        /// Should be a single character or an empty string.
        /// If it is empty then strings cannot be quoted.
        /// </summary>
        [StepProperty(4)]
        [DefaultValueExplanation("\u00FE")]
        [SingleCharacter]
        public IStep<StringStream> QuoteCharacter { get; set; } = new StringConstant(new StringStream("\u00FE"));

        /// <summary>
        /// Whether to always quote all fields and headers.
        /// </summary>
        [StepProperty(5)]
        [DefaultValueExplanation("false")]
        public IStep<bool> AlwaysQuote { get; set; } = new BoolConstant(true);

        /// <summary>
        /// The multi value delimiter character to use.
        /// Should be a single character or an empty string.
        /// If it is empty then fields cannot have multiple fields.
        /// </summary>
        [StepProperty(6)]
        [DefaultValueExplanation("")]
        [SingleCharacter]
        public IStep<StringStream> MultiValueDelimiter { get; set; } = new StringConstant(new StringStream("|"));

        /// <summary>
        /// The format to use for DateTime fields.
        /// </summary>
        [StepProperty(7)]
        [DefaultValueExplanation("O - ISO 8601 compliant - e.g. 2009-06-15T13:45:30.0000000-07:00")]
        [Example("yyyy/MM/dd HH:mm:ss")]
        public IStep<StringStream> DateTimeFormat { get; set; } = new StringConstant(new StringStream("O"));
    }



    /// <summary>
    /// Write entities to a stream in concordance format.
    /// The same as ToCSV but with different default values.
    /// </summary>
    public sealed class ToConcordanceStepFactory : SimpleStepFactory<ToConcordance, StringStream>
    {
        private ToConcordanceStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ToConcordance, StringStream> Instance { get; } = new ToConcordanceStepFactory();
    }
}