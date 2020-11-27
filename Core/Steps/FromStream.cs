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
        public override async Task<Result<string, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var streamResult = await Stream.Run(stateMonad, cancellationToken);

            if (streamResult.IsFailure)
                return streamResult.ConvertFailure<string>();

            streamResult.Value.Seek(0, SeekOrigin.Begin);


            var encodingResult = await Encoding.Run(stateMonad, cancellationToken);

            if (encodingResult.IsFailure)
                return encodingResult.ConvertFailure<string>();


            using StreamReader reader = new StreamReader(streamResult.Value, encodingResult.Value.Convert());
            var text = await reader.ReadToEndAsync();

            return text;

        }

        /// <summary>
        /// The stream to read.
        /// </summary>
        [StepProperty(Order = 2)]
        [Required]
        public IStep<Stream> Stream { get; set; } = null!;

        /// <summary>
        /// How the stream is encoded.
        /// </summary>
        [StepProperty(Order = 2)]
        [DefaultValueExplanation("UTF8 no BOM")]
        public IStep<EncodingEnum> Encoding { get; set; } = new Constant<EncodingEnum>(EncodingEnum.UTF8);

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