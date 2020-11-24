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
    /// Write entities to a stream in csv format.
    /// </summary>
    public sealed class WriteCSV : CompoundStep<Stream>
    {
        /// <inheritdoc />
        public override async Task<Result<Stream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entitiesResult = await Entities.Run(stateMonad, cancellationToken);

            if (entitiesResult.IsFailure) return entitiesResult.ConvertFailure<Stream>();

            var delimiterResult = await Delimiter.Run(stateMonad, cancellationToken);

            if (delimiterResult.IsFailure) return delimiterResult.ConvertFailure<Stream>();

            var encodingResult = await Encoding.Run(stateMonad, cancellationToken);

            if (encodingResult.IsFailure) return encodingResult.ConvertFailure<Stream>();


            var result = await CSVWriter.WriteCSV(entitiesResult.Value, delimiterResult.Value, encodingResult.Value.Convert(), cancellationToken);

            return result;
        }

        /// <summary>
        /// The entities to write.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<EntityStream> Entities { get; set; } = null!;

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


        /// <inheritdoc />
        public override IStepFactory StepFactory => WriteCsvFactory.Instance;
    }

    /// <summary>
    /// Write entities to a stream in csv format.
    /// </summary>
    public sealed class WriteCsvFactory : SimpleStepFactory<WriteCSV, Stream>
    {
        private WriteCsvFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<WriteCSV, Stream> Instance { get; } = new WriteCsvFactory();
    }
}
