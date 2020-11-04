using System.Collections.Generic;
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
    /// Extracts elements from a CSV file
    /// </summary>
    public sealed class ReadCsv : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task< Result<EntityStream, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var testStreamResult = await TextStream.Run(stateMonad, cancellationToken);
            if (testStreamResult.IsFailure)
                return testStreamResult.ConvertFailure<EntityStream>();

            var delimiterResult = await Delimiter.Run(stateMonad, cancellationToken);
            if (delimiterResult.IsFailure)
                return delimiterResult.ConvertFailure<EntityStream>();

            string? commentToken;

            if (CommentToken == null)
                commentToken = null;
            else
            {
                var commentTokenResult = await CommentToken.Run(stateMonad, cancellationToken);
                if (commentTokenResult.IsFailure)
                    return commentTokenResult.ConvertFailure<EntityStream>();
                commentToken = commentTokenResult.Value;
            }

            var fieldsEnclosedInQuotesResult = await HasFieldsEnclosedInQuotes.Run(stateMonad, cancellationToken);
            if (fieldsEnclosedInQuotesResult.IsFailure)
                return fieldsEnclosedInQuotesResult.ConvertFailure<EntityStream>();

            var columnsToMapResult = await ColumnsToMap.Run(stateMonad, cancellationToken);
            if (columnsToMapResult.IsFailure)
                return columnsToMapResult.ConvertFailure<EntityStream>();

            var encodingResult = await Encoding.Run(stateMonad, cancellationToken);
            if (encodingResult.IsFailure)
                return encodingResult.ConvertFailure<EntityStream>();


            var block = CSVBlockHelper.ReadCsv(testStreamResult.Value,
                encodingResult.Value.Convert(),
                delimiterResult.Value,
                commentToken,
                fieldsEnclosedInQuotesResult.Value, new StepErrorLocation(this));

            var recordStream = new EntityStream(block);

            return recordStream;
        }


        /// <summary>
        /// The text of the CSV file.
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<Stream> TextStream { get; set; } = null!;

        /// <summary>
        /// The delimiter to use to separate rows.
        /// </summary>
        [StepProperty(Order = 2)]
        [DefaultValueExplanation(",")]
        public IStep<string> Delimiter { get; set; } = new Constant<string>(",");

        /// <summary>
        /// The token to use to indicate comments.
        /// </summary>
        [StepProperty(Order = 3)]
        [DefaultValueExplanation("Comments cannot be indicated")]
        public IStep<string>? CommentToken { get; set; }

        /// <summary>
        /// Whether CSV fields are enclosed in quotes.
        /// </summary>
        [StepProperty(Order = 4)]
        [DefaultValueExplanation("false")]
        public IStep<bool> HasFieldsEnclosedInQuotes { get; set; } = new Constant<bool>(false);

        /// <summary>
        /// The csv columns to map to result columns, in order.
        /// </summary>
        [StepProperty(Order = 5)]
        [Required]
        public IStep<List<string>> ColumnsToMap { get; set; } = null!;

        /// <summary>
        /// How the stream is encoded.
        /// </summary>
        [StepProperty(Order = 6)]
        [DefaultValueExplanation("The default encoding")]
        public IStep<EncodingEnum> Encoding { get; set; } = new Constant<EncodingEnum>(EncodingEnum.Default);

        /// <inheritdoc />
        public override IStepFactory StepFactory => ReadCsvStepFactory.Instance;
    }


    /// <summary>
    /// Extracts elements from a CSV file
    /// </summary>
    public sealed class ReadCsvStepFactory : SimpleStepFactory<ReadCsv, EntityStream>
    {
        private ReadCsvStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ReadCsv, EntityStream> Instance { get; } = new ReadCsvStepFactory();
    }
}