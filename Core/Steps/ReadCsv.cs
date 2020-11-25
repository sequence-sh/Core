using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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
    public sealed class ReadCSV : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var testStreamResult = await TextStream.Run(stateMonad, cancellationToken);
            if (testStreamResult.IsFailure)
                return testStreamResult.ConvertFailure<EntityStream>();

            var delimiterResult = await Delimiter.Run(stateMonad, cancellationToken);
            if (delimiterResult.IsFailure)
                return delimiterResult.ConvertFailure<EntityStream>();

            char? commentToken;

            if (CommentToken == null)
                commentToken = null;
            else
            {
                var commentTokenResult = await CommentToken.Run(stateMonad, cancellationToken);
                if (commentTokenResult.IsFailure)
                    return commentTokenResult.ConvertFailure<EntityStream>();

                if(commentTokenResult.Value.Length > 1)
                    return new SingleError("Comment token must be a single character.", ErrorCode.CSVError, new StepErrorLocation(this));
                commentToken = commentTokenResult.Value.Single();
            }

            var ignoreQuotesResult = await IgnoreQuotes.Run(stateMonad, cancellationToken);
            if (ignoreQuotesResult.IsFailure) return ignoreQuotesResult.ConvertFailure<EntityStream>();

            var encodingResult = await Encoding.Run(stateMonad, cancellationToken);
            if (encodingResult.IsFailure)
                return encodingResult.ConvertFailure<EntityStream>();


            var block = CSVBlockHelper.ReadCsv(testStreamResult.Value,
                encodingResult.Value.Convert(),
                ignoreQuotesResult.Value,
                delimiterResult.Value,
                commentToken);

            var recordStream = new EntityStream(block);

            return recordStream;
        }


        /// <summary>
        /// The token to use to indicate comments.
        /// Must be a single character, or null.
        /// </summary>
        [StepProperty(Order = 2)]
        [DefaultValueExplanation("Comments cannot be indicated")]
        public IStep<string>? CommentToken { get; set; } //TODO enable char property type

        /// <summary>
        /// The delimiter to use to separate rows.
        /// </summary>
        [StepProperty(Order = 3)]
        [DefaultValueExplanation(",")]
        public IStep<string> Delimiter { get; set; } = new Constant<string>(",");

        /// <summary>
        /// How the stream is encoded.
        /// </summary>
        [StepProperty(Order = 4)]
        [DefaultValueExplanation("The default encoding")]
        public IStep<EncodingEnum> Encoding { get; set; } = new Constant<EncodingEnum>(EncodingEnum.Default);

        /// <summary>
        /// If true, quotes should be treated like any other character.
        /// </summary>
        [StepProperty(Order = 5)]
        [DefaultValueExplanation("false")]
        public IStep<bool> IgnoreQuotes { get; set; } = new Constant<bool>(false);

        /// <summary>
        /// The text of the CSV file.
        /// </summary>
        [StepProperty(Order = 6)]
        [Required]
        public IStep<Stream> TextStream { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => ReadCsvStepFactory.Instance;
    }


    /// <summary>
    /// Extracts elements from a CSV file
    /// </summary>
    public sealed class ReadCsvStepFactory : SimpleStepFactory<ReadCSV, EntityStream>
    {
        private ReadCsvStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ReadCSV, EntityStream> Instance { get; } = new ReadCsvStepFactory();
    }
}