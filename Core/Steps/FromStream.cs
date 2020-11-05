using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Creates a string from a stream.
    /// </summary>
    public sealed class FromStream : CompoundStep<string>
    {
        /// <inheritdoc />
        public override async Task<Result<string, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var streamResult = await Stream.Run(stateMonad, cancellationToken);

            if (streamResult.IsFailure)
                return streamResult.ConvertFailure<string>();

            var encodingResult = await Encoding.Run(stateMonad, cancellationToken);

            if (encodingResult.IsFailure)
                return encodingResult.ConvertFailure<string>();


            using StreamReader reader = new StreamReader(streamResult.Value, encodingResult.Value.Convert());
            var text = await reader.ReadToEndAsync();

            return text;

        }

        /// <summary>
        /// How to stream is encoded.
        /// </summary>
        [StepProperty(Order = 1)]
        [DefaultValueExplanation("The default encoding")]
        public IStep<EncodingEnum> Encoding { get; set; } = new Constant<EncodingEnum>(EncodingEnum.Default);

        /// <summary>
        /// The stream to read.
        /// </summary>
        [StepProperty(Order = 2)]
        [Required]
        public IStep<Stream> Stream { get; set; } = null!;



        /// <inheritdoc />
        public override IStepFactory StepFactory => FromStreamFactory.Instance;
    }

    /// <summary>
    /// Creates a string from a stream.
    /// </summary>
    public sealed class FromStreamFactory : SimpleStepFactory<FromStream, string>
    {
        private FromStreamFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<FromStream, string> Instance { get; } = new FromStreamFactory();
    }
}